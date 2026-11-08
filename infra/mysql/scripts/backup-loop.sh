#!/bin/bash
# Periodic logical backup (consistent snapshot via --single-transaction) with retention.
# Restore: infra/scripts/restore-backup.sh <file>. Point-in-time recovery: replay binlogs after the dump (docs/04-operations.md).
set -uo pipefail

: "${MYSQL_HOST:?}" "${MYSQL_DATABASE:?}" "${MYSQL_BACKUP_USER:?}" "${MYSQL_BACKUP_PASSWORD:?}"
interval="${BACKUP_INTERVAL_SECONDS:-86400}"
retention="${BACKUP_RETENTION_DAYS:-14}"
export MYSQL_PWD="${MYSQL_BACKUP_PASSWORD}"

log() { printf '{"time":"%s","level":"%s","message":"%s"}\n' "$(date -u +%FT%TZ)" "$1" "$2"; }

while true; do
  stamp="$(date -u +%Y%m%dT%H%M%SZ)"
  target="/backups/${MYSQL_DATABASE}-${stamp}.sql.gz"
  if mysqldump -h "${MYSQL_HOST}" -u "${MYSQL_BACKUP_USER}" \
       --single-transaction --quick --routines --triggers --events \
       --no-tablespaces --set-gtid-purged=OFF --default-character-set=utf8mb4 \
       "${MYSQL_DATABASE}" | gzip -6 > "${target}.partial"; then
    mv "${target}.partial" "${target}"
    sha256sum "${target}" > "${target}.sha256"
    log info "backup written: ${target} ($(du -h "${target}" | cut -f1))"
  else
    rm -f "${target}.partial"
    log error "backup failed for ${MYSQL_DATABASE}"
  fi
  find /backups -name "${MYSQL_DATABASE}-*.sql.gz*" -mtime "+${retention}" -delete
  sleep "${interval}"
done
