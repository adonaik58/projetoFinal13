# TLS certificates

Nginx expects `fullchain.pem` and `privkey.pem` in this directory (or in `TLS_CERTS_DIR`).
`*.pem` files are git-ignored — never commit private keys.

* **Local/dev:** `./infra/scripts/generate-dev-certs.sh` creates a self-signed certificate for `localhost`.
* **Staging/production:** use certificates from your CA or Let's Encrypt (e.g. certbot with the
  `webroot`/DNS challenge) and point `TLS_CERTS_DIR` at the directory that holds them.
