#!/usr/bin/env bash
# Self-signed certificate for local HTTPS (browser will warn; trust it locally if desired).
set -euo pipefail
dir="$(cd "$(dirname "$0")/../nginx/certs" && pwd)"
host="${1:-localhost}"
openssl req -x509 -nodes -newkey rsa:2048 -days 365 \
  -keyout "$dir/privkey.pem" -out "$dir/fullchain.pem" \
  -subj "/CN=${host}" \
  -addext "subjectAltName=DNS:${host},DNS:localhost,IP:127.0.0.1" >/dev/null 2>&1
chmod 600 "$dir/privkey.pem"
echo "Certificate written to $dir (CN=${host})"
