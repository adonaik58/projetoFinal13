#!/usr/bin/env bash
# Restores a dump produced by the backup service into the stack's MySQL.
# Usage: infra/scripts/restore-backup.sh <backup-file-name-inside-volume> [--yes]
#        (list files with: docker compose exec backup ls -l /backups)
set -euo pipefail
file="${1:?usage: restore-backup.sh <file> [--yes]}"
if [[ "${2:-}" != "--yes" ]]; then
  read -r -p "This REPLACES the current database contents with ${file}. Type 'restore' to continue: " answer
  [[ "${answer}" == "restore" ]] || { echo "aborted"; exit 1; }
fi

docker compose exec -T backup sh -c "cd /backups && sha256sum -c '${file}.sha256'"
docker compose stop api
docker compose exec -T backup sh -c "gunzip -c '/backups/${file}'" \
  | docker compose exec -T mysql sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" mysql -u"$MYSQL_USER" "$MYSQL_DATABASE"'
docker compose start api
echo "Restore of ${file} completed."
