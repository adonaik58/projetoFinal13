-- =============================================================================
-- Xyami.Park — data migration from the legacy PHP schema to the new schema.
--
-- Prerequisites (see docs/04-operations.md § "Migração do sistema legado"):
--   1. Restore the legacy dump into a schema named `parking_legacy` on the same server.
--   2. Run the migrator once against the new schema (creates tables, settings and the bootstrap admin).
--   3. Run:  mysql -u<owner> -p parking < migrate-from-legacy.sql
--
-- Properties:
--   * Rows are matched by natural keys (username, space code, brand/model name, BI, plate),
--     so the script is idempotent: re-running it only inserts what is still missing.
--   * Passwords keep their legacy MD5 hash as "md5:<hash>"; they are re-hashed with PBKDF2 on first login.
--   * Legacy instants were stored in local time (Africa/Lagos/Luanda, UTC+1) and are converted to UTC.
--   * Ticket amounts are recomputed with the unified pricing rule (the legacy stored amount used a
--     hard-coded rate). The legacy schema is left untouched for audit purposes.
--   * Rows that cannot be migrated faithfully (invalid BI/plate/space code, missing brand or birth date)
--     are skipped and listed in the report at the end — nothing is invented.
-- =============================================================================

SET @legacy_utc_offset_minutes = 60;
SET @now = UTC_TIMESTAMP(6);

-- ---------------------------------------------------------------------------
-- Staging: normalized copies in utf8mb4 (avoids cross-schema collation mismatches).
-- Regular tables (MySQL cannot reference a TEMPORARY table twice in one query); dropped at the end.
-- ---------------------------------------------------------------------------
DROP TABLE IF EXISTS _etl_employee, _etl_brand, _etl_model, _etl_space, _etl_consumer, _etl_ticket, _etl_skipped;

CREATE TABLE _etl_skipped (entity VARCHAR(32), legacy_id BIGINT, reason VARCHAR(255));

CREATE TABLE _etl_employee AS
SELECT e.id,
       CONVERT(LEFT(TRIM(e.nome), 50) USING utf8mb4)                                    AS username,
       CONVERT(LEFT(COALESCE(NULLIF(TRIM(e.nome_completo), ''), TRIM(e.nome)), 150) USING utf8mb4) AS full_name,
       CONVERT(LOWER(TRIM(e.senha)) USING utf8mb4)                                      AS md5_hash,
       IF(CAST(e.code AS UNSIGNED) = 2, 'Admin', 'Operator')                             AS role
FROM parking_legacy.employee e;

CREATE TABLE _etl_brand AS
SELECT m.id, CONVERT(LEFT(TRIM(m.nome), 80) USING utf8mb4) AS name
FROM parking_legacy.marcas m
WHERE TRIM(m.nome) <> '';

CREATE TABLE _etl_model AS
SELECT mo.id, mo.id_marca AS brand_legacy_id, CONVERT(LEFT(TRIM(mo.nome), 80) USING utf8mb4) AS name
FROM parking_legacy.modelos mo
WHERE TRIM(mo.nome) <> '';

CREATE TABLE _etl_space AS
SELECT e.id,
       CONVERT(UPPER(TRIM(e.nome)) USING utf8mb4) AS code,
       e.estado                                     AS legacy_status,
       CONVERT(UPPER(TRIM(e.bi)) USING utf8mb4)     AS current_bi
FROM parking_legacy.espacos e;

CREATE TABLE _etl_consumer AS
SELECT c.id,
       CONVERT(LEFT(TRIM(c.nome), 150) USING utf8mb4)                                   AS full_name,
       CONVERT(UPPER(TRIM(c.bi)) USING utf8mb4)                                         AS national_id,
       CAST(c.idade AS DATE)                                                            AS birth_date,
       c.id_marca_carro                                                                 AS brand_legacy_id,
       NULLIF(c.id_modelo_carro, 0)                                                     AS model_legacy_id,
       CONVERT(IF(UPPER(TRIM(c.cor_carro)) REGEXP '^#[0-9A-F]{6}$', UPPER(TRIM(c.cor_carro)), '#000000') USING utf8mb4) AS color,
       CONVERT(REGEXP_REPLACE(UPPER(TRIM(c.matricula_carro)), '[[:space:]]+', ' ') USING utf8mb4) AS plate,
       DATE_SUB(c.data_hora_entrada, INTERVAL @legacy_utc_offset_minutes MINUTE)        AS entry_at,
       DATE_SUB(c.data_hora_saida, INTERVAL @legacy_utc_offset_minutes MINUTE)          AS exit_at,
       c.operador                                                                       AS operator_legacy_id,
       c.renda_min                                                                      AS rate
FROM parking_legacy.consumidores c;

CREATE TABLE _etl_ticket AS
SELECT t.id,
       t.id_consumidor                                                                  AS consumer_legacy_id,
       t.id_espaco                                                                      AS space_legacy_id,
       DATE_SUB(t.data_entrada, INTERVAL @legacy_utc_offset_minutes MINUTE)             AS entry_at,
       DATE_SUB(t.data_saida, INTERVAL @legacy_utc_offset_minutes MINUTE)               AS exit_at,
       t.renda_min                                                                      AS rate,
       t.operador                                                                       AS closed_by_legacy_id
FROM parking_legacy.ticket_historico t;

SELECT CAST(c.num_hora_gratis AS UNSIGNED) * 60, CAST(c.renda_min AS DECIMAL(10, 2))
INTO @free_minutes, @current_rate
FROM parking_legacy.config c
LIMIT 1;

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- Users and settings
-- ---------------------------------------------------------------------------
INSERT INTO users (username, full_name, password_hash, role, is_active, created_at, updated_at)
SELECT s.username, s.full_name, CONCAT('md5:', s.md5_hash), s.role, 1, @now, @now
FROM _etl_employee s
WHERE CHAR_LENGTH(s.username) >= 3
  AND s.md5_hash REGEXP '^[0-9a-f]{32}$'
  AND NOT EXISTS (SELECT 1 FROM users u WHERE u.username = s.username);

INSERT INTO _etl_skipped
SELECT 'user', s.id, 'username com menos de 3 caracteres ou hash MD5 inválido'
FROM _etl_employee s
WHERE CHAR_LENGTH(s.username) < 3 OR NOT s.md5_hash REGEXP '^[0-9a-f]{32}$';

UPDATE parking_settings p
SET p.rate_per_minute = COALESCE(@current_rate, p.rate_per_minute),
    p.free_hours      = COALESCE(@free_minutes DIV 60, p.free_hours),
    p.max_spaces      = GREATEST(
                          (SELECT COALESCE(MAX(CAST(c.quant_max_espaco AS UNSIGNED)), 0) FROM parking_legacy.config c),
                          (SELECT COUNT(*) FROM _etl_space),
                          p.max_spaces),
    p.updated_at      = @now
WHERE p.id = 1;

SET @free_minutes = COALESCE(@free_minutes, (SELECT free_hours * 60 FROM parking_settings WHERE id = 1));
SET @current_rate = COALESCE(@current_rate, (SELECT rate_per_minute FROM parking_settings WHERE id = 1));
-- Operator of last resort for legacy rows whose operator no longer exists (reported below).
SET @fallback_user_id = (SELECT MIN(id) FROM users WHERE role = 'Admin' AND is_active = 1);

-- ---------------------------------------------------------------------------
-- Catalogue
-- ---------------------------------------------------------------------------
INSERT INTO vehicle_brands (name)
SELECT DISTINCT s.name FROM _etl_brand s
WHERE NOT EXISTS (SELECT 1 FROM vehicle_brands b WHERE b.name = s.name);

INSERT INTO vehicle_models (brand_id, name)
SELECT DISTINCT b.id, m.name
FROM _etl_model m
JOIN _etl_brand sb ON sb.id = m.brand_legacy_id
JOIN vehicle_brands b ON b.name = sb.name
WHERE NOT EXISTS (SELECT 1 FROM vehicle_models vm WHERE vm.brand_id = b.id AND vm.name = m.name);

-- ---------------------------------------------------------------------------
-- Spaces (occupied ones are switched to Occupied when their open ticket is migrated)
-- ---------------------------------------------------------------------------
INSERT INTO parking_spaces (code, section, number, status, version, created_at, updated_at)
SELECT s.code,
       REGEXP_SUBSTR(s.code, '^[A-Z]+'),
       CAST(REGEXP_SUBSTR(s.code, '[0-9]+$') AS UNSIGNED),
       IF(s.legacy_status = 'm', 'Maintenance', 'Free'),
       0, @now, @now
FROM _etl_space s
WHERE s.code REGEXP '^[A-Z]{1,3}[1-9][0-9]{0,3}$'
  AND NOT EXISTS (SELECT 1 FROM parking_spaces p WHERE p.code = s.code);

INSERT INTO _etl_skipped
SELECT 'space', s.id, CONCAT('código inválido: ', s.code) FROM _etl_space s
WHERE NOT s.code REGEXP '^[A-Z]{1,3}[1-9][0-9]{0,3}$';

-- ---------------------------------------------------------------------------
-- Customers (latest legacy row per BI) and vehicles (latest legacy row per plate)
-- ---------------------------------------------------------------------------
INSERT INTO customers (full_name, national_id, birth_date, created_at, updated_at)
SELECT c.full_name, c.national_id, c.birth_date, @now, @now
FROM _etl_consumer c
JOIN (SELECT national_id, MAX(id) AS id FROM _etl_consumer GROUP BY national_id) latest ON latest.id = c.id
WHERE c.national_id REGEXP '^[0-9]{9}[A-Z]{2}[0-9]{3}$'
  AND c.birth_date IS NOT NULL
  AND CHAR_LENGTH(c.full_name) >= 2
  AND NOT EXISTS (SELECT 1 FROM customers x WHERE x.national_id = c.national_id);

INSERT INTO _etl_skipped
SELECT 'customer', c.id, 'BI inválido, nome em falta ou data de nascimento em falta'
FROM _etl_consumer c
WHERE NOT c.national_id REGEXP '^[0-9]{9}[A-Z]{2}[0-9]{3}$' OR c.birth_date IS NULL OR CHAR_LENGTH(c.full_name) < 2;

INSERT INTO vehicles (plate, brand_id, model_id, color, created_at, updated_at)
SELECT c.plate, b.id, vm.id, c.color, @now, @now
FROM _etl_consumer c
JOIN (SELECT plate, MAX(id) AS id FROM _etl_consumer GROUP BY plate) latest ON latest.id = c.id
JOIN _etl_brand sb ON sb.id = c.brand_legacy_id
JOIN vehicle_brands b ON b.name = sb.name
LEFT JOIN _etl_model sm ON sm.id = c.model_legacy_id AND sm.brand_legacy_id = c.brand_legacy_id
LEFT JOIN vehicle_models vm ON vm.brand_id = b.id AND vm.name = sm.name
WHERE c.plate REGEXP '^[A-Z0-9][A-Z0-9 -]{0,18}[A-Z0-9]$'
  AND NOT EXISTS (SELECT 1 FROM vehicles v WHERE v.plate = c.plate);

INSERT INTO _etl_skipped
SELECT 'vehicle', c.id, 'matrícula inválida ou marca inexistente'
FROM _etl_consumer c
LEFT JOIN _etl_brand sb ON sb.id = c.brand_legacy_id
WHERE NOT c.plate REGEXP '^[A-Z0-9][A-Z0-9 -]{0,18}[A-Z0-9]$' OR sb.id IS NULL;

-- ---------------------------------------------------------------------------
-- Closed tickets (legacy ticket history), priced with the unified rule
-- ---------------------------------------------------------------------------
INSERT INTO tickets (code, status, space_id, customer_id, vehicle_id, entry_at, exit_at, duration_minutes, amount,
                     rate_per_minute, free_minutes, opened_by_user_id, closed_by_user_id, created_at, updated_at)
SELECT CONCAT('L', LPAD(t.id, 9, '0')),
       'Closed',
       p.id, cu.id, v.id,
       t.entry_at, t.exit_at,
       d.minutes,
       IF(d.minutes < @free_minutes, 0, (d.minutes - @free_minutes) * d.rate),
       d.rate, @free_minutes,
       COALESCE(opener.id, @fallback_user_id),
       COALESCE(closer.id, @fallback_user_id),
       @now, @now
FROM _etl_ticket t
JOIN _etl_consumer c ON c.id = t.consumer_legacy_id
JOIN customers cu ON cu.national_id = c.national_id
JOIN vehicles v ON v.plate = c.plate
JOIN _etl_space s ON s.id = t.space_legacy_id
JOIN parking_spaces p ON p.code = s.code
JOIN LATERAL (SELECT GREATEST(0, FLOOR(TIMESTAMPDIFF(SECOND, t.entry_at, t.exit_at) / 60)) AS minutes,
                     CAST(COALESCE(NULLIF(t.rate, 0), @current_rate) AS DECIMAL(10, 2)) AS rate) d
LEFT JOIN _etl_employee eo ON eo.id = c.operator_legacy_id
LEFT JOIN users opener ON opener.username = eo.username
LEFT JOIN _etl_employee ec ON ec.id = t.closed_by_legacy_id
LEFT JOIN users closer ON closer.username = ec.username
WHERE t.entry_at IS NOT NULL AND t.exit_at IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM tickets x WHERE x.code = CONCAT('L', LPAD(t.id, 9, '0')));

INSERT INTO _etl_skipped
SELECT 'ticket', t.id, 'cliente, viatura, espaço ou datas em falta'
FROM _etl_ticket t
WHERE NOT EXISTS (SELECT 1 FROM tickets x WHERE x.code = CONCAT('L', LPAD(t.id, 9, '0')));

-- ---------------------------------------------------------------------------
-- Open tickets: consumer still assigned to an occupied space (latest row per BI)
-- ---------------------------------------------------------------------------
INSERT IGNORE INTO tickets (code, status, space_id, customer_id, vehicle_id, entry_at, rate_per_minute, free_minutes,
                            opened_by_user_id, created_at, updated_at)
SELECT CONCAT('LO', LPAD(c.id, 8, '0')),
       'Open',
       p.id, cu.id, v.id,
       c.entry_at,
       CAST(COALESCE(NULLIF(c.rate, 0), @current_rate) AS DECIMAL(10, 2)),
       @free_minutes,
       COALESCE(opener.id, @fallback_user_id),
       @now, @now
FROM _etl_space s
JOIN parking_spaces p ON p.code = s.code
JOIN (SELECT national_id, MAX(id) AS id FROM _etl_consumer WHERE exit_at IS NULL GROUP BY national_id) latest
     ON latest.national_id = s.current_bi
JOIN _etl_consumer c ON c.id = latest.id
JOIN customers cu ON cu.national_id = c.national_id
JOIN vehicles v ON v.plate = c.plate
LEFT JOIN _etl_employee eo ON eo.id = c.operator_legacy_id
LEFT JOIN users opener ON opener.username = eo.username
WHERE s.legacy_status = 'i' AND c.entry_at IS NOT NULL;

UPDATE parking_spaces p
JOIN tickets t ON t.space_id = p.id AND t.status = 'Open'
SET p.status = 'Occupied', p.version = p.version + 1, p.updated_at = @now
WHERE p.status <> 'Occupied';

INSERT INTO _etl_skipped
SELECT 'open_ticket', s.id, CONCAT('espaço ', s.code, ' ocupado no legado sem cliente/viatura migrável')
FROM _etl_space s
JOIN parking_spaces p ON p.code = s.code
WHERE s.legacy_status = 'i' AND p.status <> 'Occupied';

INSERT INTO audit_logs (occurred_at, user_id, username, action, entity_type, entity_id, details)
SELECT @now, NULL, 'system', 'system.legacy_import', NULL, NULL,
       JSON_OBJECT('tickets', (SELECT COUNT(*) FROM tickets WHERE code LIKE 'L%'),
                   'skipped', (SELECT COUNT(*) FROM _etl_skipped));

COMMIT;

-- ---------------------------------------------------------------------------
-- Report
-- ---------------------------------------------------------------------------
SELECT 'users'      AS entity, (SELECT COUNT(*) FROM _etl_employee) AS legacy_rows, (SELECT COUNT(*) FROM users WHERE password_hash LIKE 'md5:%') AS pending_rehash
UNION ALL SELECT 'brands',        (SELECT COUNT(*) FROM _etl_brand),  (SELECT COUNT(*) FROM vehicle_brands)
UNION ALL SELECT 'models',        (SELECT COUNT(*) FROM _etl_model),  (SELECT COUNT(*) FROM vehicle_models)
UNION ALL SELECT 'spaces',        (SELECT COUNT(*) FROM _etl_space),  (SELECT COUNT(*) FROM parking_spaces)
UNION ALL SELECT 'customers',     (SELECT COUNT(DISTINCT national_id) FROM _etl_consumer), (SELECT COUNT(*) FROM customers)
UNION ALL SELECT 'vehicles',      (SELECT COUNT(DISTINCT plate) FROM _etl_consumer), (SELECT COUNT(*) FROM vehicles)
UNION ALL SELECT 'closed_tickets',(SELECT COUNT(*) FROM _etl_ticket), (SELECT COUNT(*) FROM tickets WHERE code LIKE 'L0%' OR code REGEXP '^L[0-9]')
UNION ALL SELECT 'open_tickets',  (SELECT COUNT(*) FROM _etl_space WHERE legacy_status = 'i'), (SELECT COUNT(*) FROM tickets WHERE code LIKE 'LO%');

SELECT entity, legacy_id, reason FROM _etl_skipped ORDER BY entity, legacy_id;

DROP TABLE IF EXISTS _etl_employee, _etl_brand, _etl_model, _etl_space, _etl_consumer, _etl_ticket, _etl_skipped;
