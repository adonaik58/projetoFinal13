#!/usr/bin/env bash
# Zero-downtime rolling deploy on a Docker Compose host, with automatic rollback.
#   Usage (from the deployment directory): ./infra/scripts/deploy.sh <image-tag>
# Steps: pull → migrate (one-shot) → start new API replicas next to the old ones → wait until healthy →
#        retire old replicas → refresh web/proxy → verify through the proxy. Any failure rolls the images back.
# Database migrations must be backward compatible (expand/contract) because the previous version keeps
# serving traffic during the rollout and after a rollback.
set -euo pipefail

NEW_TAG="${1:?usage: deploy.sh <image-tag>}"
COMPOSE=(docker compose -f docker-compose.yml)
ENV_FILE=.env
HEALTH_URL="${HEALTH_URL:-https://localhost/health/ready}"

log() { printf '[deploy %s] %s\n' "$(date -u +%FT%TZ)" "$*"; }
current_tag() { grep -E '^IMAGE_TAG=' "$ENV_FILE" | cut -d= -f2; }
set_tag() { sed -i.bak -E "s/^IMAGE_TAG=.*/IMAGE_TAG=$1/" "$ENV_FILE" && rm -f "$ENV_FILE.bak"; }
replicas() { grep -E '^API_REPLICAS=' "$ENV_FILE" | cut -d= -f2 || echo 2; }

wait_healthy() { # container ids...
  for _ in $(seq 1 60); do
    local pending=0
    for id in "$@"; do
      [[ "$(docker inspect -f '{{.State.Health.Status}}' "$id" 2>/dev/null)" == healthy ]] || pending=1
    done
    (( pending == 0 )) && return 0
    sleep 3
  done
  return 1
}

roll_api() {
  local target old new desired
  desired="$(replicas)"
  mapfile -t old < <("${COMPOSE[@]}" ps -q api)
  target=$(( ${#old[@]} + desired ))
  log "starting ${desired} new API replica(s) alongside ${#old[@]} running"
  "${COMPOSE[@]}" up -d --no-deps --no-recreate --scale "api=${target}" api
  mapfile -t new < <(comm -13 <(printf '%s\n' "${old[@]}" | sort) <("${COMPOSE[@]}" ps -q api | sort))
  if ! wait_healthy "${new[@]}"; then
    log "new replicas unhealthy; removing them"
    docker rm -f "${new[@]}" >/dev/null
    return 1
  fi
  if (( ${#old[@]} > 0 )); then
    log "retiring old replicas"
    docker stop -t 30 "${old[@]}" >/dev/null && docker rm "${old[@]}" >/dev/null
  fi
}

deploy_tag() {
  set_tag "$1"
  "${COMPOSE[@]}" pull migrator api web
  log "running database migrations"
  "${COMPOSE[@]}" run --rm migrator
  roll_api
  "${COMPOSE[@]}" up -d --no-deps web
  "${COMPOSE[@]}" up -d --no-deps proxy
  "${COMPOSE[@]}" exec -T proxy nginx -s reload
  for _ in $(seq 1 20); do
    curl -fsSk "$HEALTH_URL" >/dev/null && return 0
    sleep 3
  done
  return 1
}

PREVIOUS_TAG="$(current_tag)"
log "deploying ${NEW_TAG} (current: ${PREVIOUS_TAG})"
if deploy_tag "$NEW_TAG"; then
  log "deploy of ${NEW_TAG} succeeded"
  docker image prune -f >/dev/null
  exit 0
fi

log "deploy of ${NEW_TAG} FAILED — rolling back to ${PREVIOUS_TAG}"
set_tag "$PREVIOUS_TAG"
roll_api || true
"${COMPOSE[@]}" up -d --no-deps web proxy
exit 1
