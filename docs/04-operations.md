# Operações

## 1. Ambientes e pipeline

| Ambiente | Gatilho | Aprovação | Configuração |
|---|---|---|---|
| CI | qualquer PR / push | — | `.github/workflows/ci.yml`, `codeql.yml` |
| Staging | push em `main` | automática | GitHub Environment `staging` |
| Produção | tag `vX.Y.Z` (ou execução manual) | revisores do Environment `production` | GitHub Environment `production` |

**CI** (`ci.yml`): restore → `dotnet format` → build com analisadores (warnings = erros) → verificação de migrations
pendentes → testes unitários → testes de integração/API (Testcontainers) → auditoria de dependências →
lint/format/types/testes/build do frontend → `npm audit` → Trivy (vulnerabilidades, segredos, IaC) →
stack completa em Docker + Playwright + k6 smoke. **CodeQL** analisa C# e TypeScript.

**CD** (`cd.yml` + `deploy.yml`): build e publicação das imagens `api`/`web` no GHCR (SBOM + proveniência),
scan Trivy da imagem, sincronização de `docker-compose.yml` + `infra/` para o servidor, deploy, verificação externa
de `/health/ready`.

Configuração por Environment no GitHub: variáveis `DEPLOY_HOST`, `DEPLOY_USER`, `DEPLOY_PATH`, `PUBLIC_URL`;
segredos `DEPLOY_SSH_KEY`, `DEPLOY_KNOWN_HOSTS`. Os segredos da aplicação ficam **apenas** no `.env` do servidor
(com `IMAGE_REGISTRY=ghcr.io/<owner>/xyami-park`).

## 2. Deploy e rollback

`infra/scripts/deploy.sh <tag>` (executado pelo pipeline no servidor):

1. Atualiza `IMAGE_TAG` no `.env` e faz `pull` das imagens.
2. Corre o `migrator` (one-shot). Falha → aborta antes de tocar no tráfego.
3. **Rolling update** da API: arranca as novas réplicas ao lado das antigas, espera que fiquem `healthy`,
   e só então retira as antigas (o Nginx re-resolve o DNS a cada 10 s e ignora réplicas com falhas).
4. Atualiza `web` e recarrega o `proxy`.
5. Verifica `https://localhost/health/ready` através do proxy.
6. Qualquer falha nos passos 3–5 → **rollback automático** para a tag anterior.

Rollback manual: `./infra/scripts/deploy.sh <tag-anterior>`.

> As migrations têm de ser **compatíveis com a versão anterior** (padrão *expand/contract*: adicionar colunas
> nulas/tabelas primeiro, remover só numa versão seguinte), porque a versão antiga continua a servir pedidos
> durante o rollout e após um rollback. O rollback não reverte o esquema.

## 3. Backups e recuperação

| Mecanismo | Detalhe |
|---|---|
| Backup lógico | serviço `backup`: `mysqldump --single-transaction` (consistente, sem bloquear escritas), comprimido, com `sha256`, a cada `BACKUP_INTERVAL_SECONDS` (24 h), retenção `BACKUP_RETENTION_DAYS` (14 dias), volume `mysql-backups` |
| Binary log | ativo (`ROW`, 7 dias) — permite recuperação para um instante entre dois backups |
| Cópia externa | sincronize o volume `mysql-backups` para armazenamento fora do servidor (ex.: `rclone`/S3) — **obrigatório em produção** |

Listar e restaurar:

```bash
docker compose exec backup ls -lh /backups
./infra/scripts/restore-backup.sh parking-20261107T030000Z.sql.gz     # verifica o sha256, pára a API, restaura, reinicia
```

Recuperação para um instante (PITR): restaurar o último dump anterior ao incidente e aplicar os binlogs até ao instante desejado:

```bash
docker compose exec mysql sh -c 'mysqlbinlog --start-datetime="2026-11-07 03:00:00" --stop-datetime="2026-11-07 10:41:00" /var/lib/mysql/mysql-bin.0000* | MYSQL_PWD="$MYSQL_ROOT_PASSWORD" mysql -uroot'
```

Teste a restauração periodicamente (ex.: mensalmente num ambiente de staging).

## 4. Migração de dados do sistema legado

O script [`infra/mysql/legacy/migrate-from-legacy.sql`](../infra/mysql/legacy/migrate-from-legacy.sql) copia o esquema
PHP (`employee`, `config`, `marcas`, `modelos`, `espacos`, `consumidores`, `ticket_historico`) para o novo esquema.

1. Restaurar o dump legado num esquema `parking_legacy` no mesmo servidor MySQL.
2. Arrancar a stack (o `migrator` cria o esquema novo, a tarifa e o administrador inicial).
   Use um `BOOTSTRAP_ADMIN_USERNAME` **diferente** dos utilizadores legados (ex.: `setup-admin`): se coincidir,
   prevalece a conta nova e a conta legada com esse nome não é importada.
3. Executar como dono do esquema:
   `docker compose exec -T mysql sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" mysql -u"$MYSQL_USER" parking --table' < infra/mysql/legacy/migrate-from-legacy.sql`
4. Ler o relatório final (contagens origem/destino e lista de registos ignorados com o motivo).

Comportamento: correspondência por chaves naturais (idempotente — pode ser repetido); senhas mantêm o hash MD5
como `md5:<hash>` e são convertidas para PBKDF2 no primeiro login; instantes convertidos de UTC+1 para UTC;
valores dos tickets **recalculados** com a regra unificada (o valor gravado pelo legado usava tarifa fixa — erro B1);
registos inválidos (BI, matrícula ou código de espaço inválidos, marca ou data de nascimento em falta) são
reportados, nunca inventados. A base legada não é alterada.

Validado com um esquema legado de teste: 2 tickets fechados + 1 aberto migrados, espaço ocupado e em manutenção
preservados, 4 registos inválidos reportados, segunda execução sem alterações.

## 5. Escala e desempenho

* **API stateless**: escala horizontal com `API_REPLICAS`/`--scale api=N` (Compose) ou HPA (Kubernetes).
* **Pool de ligações**: `Maximum Pool Size` por réplica (`MYSQL_MAX_POOL_SIZE`, 50); garantir
  `réplicas × pool < max_connections` (300 em `infra/mysql/conf.d/parking.cnf`).
* **Cache** (Redis, via HybridCache com proteção contra *stampede*):

  | Chave | TTL | Invalidação |
  |---|---|---|
  | `settings:current` | 1 h | atualização da configuração |
  | `catalog:brands`, `catalog:brands:{id}:models` | 6 h | criação de marca/modelo |
  | `spaces:board` (só Redis, sem L1) | 30 s | check-in, check-out, alterações de espaços |
  | `dashboard:summary:*`, `dashboard:series:*` | 30–60 s | check-in, check-out, alterações de espaços |

* **Índices** desenhados para as consultas reais (ver [02-architecture.md §7](02-architecture.md)); listas paginadas no servidor.
* Concorrência resolvida pela base de dados (índices únicos sobre colunas geradas + *optimistic concurrency*),
  sem locks distribuídos.

## 6. Monitorização

| Sinal | Onde |
|---|---|
| Logs | JSON estruturado (Serilog / Nginx) em stdout, com `CorrelationId`; `docker compose logs -f api` |
| Correlação | cabeçalho `X-Correlation-ID` (gerado no Nginx ou pelo cliente), devolvido em todas as respostas e incluído nos erros |
| Métricas | OpenTelemetry → Collector → Prometheus; dashboard Grafana "Xyami.Park — API" (pedidos, erros, latência p95 por rota, distribuição por réplica, check-ins/outs, receita, logins falhados) |
| Traces | OpenTelemetry → Jaeger (pedido HTTP → comando/consulta CQRS) |
| Alertas | `infra/observability/alerts.yml`: API sem métricas, erros 5xx > 5 %, p95 > 1 s, picos de logins falhados |
| Health | `/health/live` (processo) e `/health/ready` (MySQL + Redis) — usados pelo Compose, pelo deploy e pelo Kubernetes |

## 7. Rotação de segredos

| Segredo | Procedimento |
|---|---|
| `JWT_SIGNING_KEY` | alterar no `.env` e reiniciar a API: os access tokens (15 min) deixam de ser válidos; as sessões renovam-se com o refresh token |
| Senhas MySQL | `ALTER USER ... IDENTIFIED BY ...` e atualizar o `.env`; reiniciar `api`/`backup` |
| `REDIS_PASSWORD` | atualizar o `.env` e `docker compose up -d redis api` (o Redis não guarda dados de negócio) |
| Certificados TLS | substituir os ficheiros em `TLS_CERTS_DIR` e `docker compose exec proxy nginx -s reload` |
