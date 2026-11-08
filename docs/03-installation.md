# Instalação, configuração e execução

## 1. Requisitos

| Ferramenta | Versão | Uso |
|---|---|---|
| Docker Engine / Desktop + Compose v2 | ≥ 24 / ≥ 2.24 | stack completa, testes de integração (Testcontainers) |
| .NET SDK | 10.0 (ver `backend/global.json`) | desenvolvimento do backend |
| Node.js | ≥ 22.12 | desenvolvimento do frontend |
| OpenSSL | qualquer | certificado TLS de desenvolvimento |

## 2. Configuração (`.env`)

Toda a configuração é feita por variáveis de ambiente; o ficheiro [`.env.example`](../.env.example) lista-as todas
com comentários e **não contém credenciais**. Copie-o para `.env` (ignorado pelo git) e preencha os segredos:

```bash
cp .env.example .env
openssl rand -base64 36 | tr -d '/+=' | cut -c1-32   # gere um valor por segredo
```

| Variável | Descrição |
|---|---|
| `MYSQL_ROOT_PASSWORD` | root do MySQL (só usado na inicialização) |
| `MYSQL_USER` / `MYSQL_PASSWORD` | dono do esquema — usado **apenas** pelo `migrator` (DDL) |
| `MYSQL_APP_USER` / `MYSQL_APP_PASSWORD` | utilizador da API — só `SELECT/INSERT/UPDATE/DELETE` |
| `MYSQL_BACKUP_USER` / `MYSQL_BACKUP_PASSWORD` | utilizador de backups — só leitura |
| `REDIS_PASSWORD` | senha do Redis |
| `JWT_SIGNING_KEY` | chave HMAC dos access tokens (≥ 32 caracteres) |
| `BOOTSTRAP_ADMIN_USERNAME` / `BOOTSTRAP_ADMIN_PASSWORD` | primeiro administrador (só quando a tabela de utilizadores está vazia) |
| `BOOTSTRAP_RATE_PER_MINUTE`, `BOOTSTRAP_FREE_HOURS`, `BOOTSTRAP_MAX_SPACES` | tarifa e capacidade iniciais |
| `PUBLIC_HOST`, `HTTP_PORT`, `HTTPS_PORT`, `TLS_CERTS_DIR` | endpoint público e certificados |
| `API_REPLICAS` | número de réplicas da API atrás do load balancer |
| `RATE_LIMIT_*`, `EDGE_*` | limites de pedidos (API/Redis e borda/Nginx) |
| `OPENAPI_ENABLED` | expõe `/swagger` e `/openapi/v1.json` (manter `false` em produção) |
| `BACKUP_INTERVAL_SECONDS`, `BACKUP_RETENTION_DAYS` | agendamento e retenção dos backups |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | ativa a exportação de traces/métricas (perfil `observability`) |

Os scripts de inicialização do MySQL (`infra/mysql/init`) só correm na **primeira** inicialização do volume;
para alterar senhas depois disso, altere-as no MySQL e no `.env`.

## 3. Stack completa (produção local, staging, produção)

```bash
./infra/scripts/generate-dev-certs.sh            # ou coloque fullchain.pem/privkey.pem reais em TLS_CERTS_DIR
docker compose -f docker-compose.yml up -d --build --wait
docker compose -f docker-compose.yml ps
```

Ordem de arranque garantida pelo Compose: `mysql`/`redis` saudáveis → `migrator` (migrations + bootstrap, termina)
→ `api` ×`API_REPLICAS` (readiness: MySQL + Redis) → `web` → `proxy`.

| URL | Serviço |
|---|---|
| `https://<PUBLIC_HOST>/` | aplicação |
| `https://<PUBLIC_HOST>/swagger` | documentação da API (com `OPENAPI_ENABLED=true`) |
| `https://<PUBLIC_HOST>/health/ready` | readiness (MySQL + Redis) |
| `http://localhost:3000` / `:9090` / `:16686` | Grafana / Prometheus / Jaeger (perfil `observability`, só localhost) |

Escalar a API: `docker compose up -d --scale api=4` (o Nginx descobre novas réplicas por DNS em ≤ 10 s).

## 4. Desenvolvimento

```bash
# 1) Dependências (MySQL e Redis publicados em 127.0.0.1 pelo docker-compose.override.yml)
docker compose up -d mysql redis

# 2) API em http://localhost:8080 — aplica migrations e cria o admin de desenvolvimento no arranque
cd backend && dotnet run --project src/Parking.Api --launch-profile http

# 3) Frontend em http://localhost:5173 (encaminha /api para :8080)
cd frontend && npm install && npm run dev
```

A configuração de desenvolvimento está em `backend/src/Parking.Api/appsettings.Development.json`
(credenciais **apenas locais**: `admin` / `Admin12345`; o MySQL usa o utilizador `parking` com a senha
`parking_dev_password` e o Redis `redis_dev_password`, que devem coincidir com o `.env` local).

Comandos úteis:

```bash
cd backend
dotnet build                                   # analisadores + warnings como erros
dotnet test tests/Parking.UnitTests            # unitários
dotnet test tests/Parking.IntegrationTests     # integração (requer Docker)
dotnet tool restore
dotnet ef migrations add <Nome> --project src/Parking.Infrastructure --startup-project src/Parking.Infrastructure --output-dir Persistence/Migrations
dotnet format                                  # formatação

cd frontend
npm run lint && npm run typecheck && npm test && npm run build
```

## 5. Kubernetes

Ver [`infra/k8s/README.md`](../infra/k8s/README.md): manifestos `kustomize` (Deployment + HPA + PDB da API,
Job de migrations, web, Ingress com TLS) para quando a escala o justificar.
