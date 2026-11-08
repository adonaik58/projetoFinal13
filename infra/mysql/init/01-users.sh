#!/bin/bash
# Runs once, on first initialisation of the data directory.
# MYSQL_USER (created by the image) owns the schema and is used only by the migrator.
# The API connects as a DML-only user; backups use a read-only user (least privilege).
set -euo pipefail

: "${MYSQL_DATABASE:?}" "${MYSQL_APP_USER:?}" "${MYSQL_APP_PASSWORD:?}" "${MYSQL_BACKUP_USER:?}" "${MYSQL_BACKUP_PASSWORD:?}"

MYSQL_PWD="${MYSQL_ROOT_PASSWORD}" mysql --protocol=socket -uroot <<SQL
CREATE USER IF NOT EXISTS '${MYSQL_APP_USER}'@'%' IDENTIFIED BY '${MYSQL_APP_PASSWORD}';
GRANT SELECT, INSERT, UPDATE, DELETE ON \`${MYSQL_DATABASE}\`.* TO '${MYSQL_APP_USER}'@'%';

CREATE USER IF NOT EXISTS '${MYSQL_BACKUP_USER}'@'%' IDENTIFIED BY '${MYSQL_BACKUP_PASSWORD}';
GRANT SELECT, SHOW VIEW, TRIGGER, EVENT, LOCK TABLES ON \`${MYSQL_DATABASE}\`.* TO '${MYSQL_BACKUP_USER}'@'%';
GRANT PROCESS, REPLICATION CLIENT ON *.* TO '${MYSQL_BACKUP_USER}'@'%';

FLUSH PRIVILEGES;
SQL
