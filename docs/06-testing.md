# Testes e qualidade

## 1. Pirâmide de testes

| Nível | Ferramentas | Onde | O que cobre |
|---|---|---|---|
| Unitários (backend) | xUnit, NSubstitute | `backend/tests/Parking.UnitTests` | regra de preço, invariantes de domínio (tickets, espaços, BI, matrículas, tarifa, permissões), validadores, pipeline CQRS, hashing (PBKDF2 e MD5 legado), JWT, refresh tokens |
| Integração / API | xUnit, `WebApplicationFactory`, Testcontainers (MySQL 8.4 + Redis 7.4 reais), `FakeTimeProvider` | `backend/tests/Parking.IntegrationTests` | pipeline HTTP completo: auth (cookie, rotação, reutilização, CSRF, lockout, upgrade MD5), RBAC, check-in/out com preço determinístico, **concorrência** (6 check-ins simultâneos → 1 aceite), idempotência, paginação/filtros, capacidade, invalidação de cache, rate limiting, ProblemDetails, OpenAPI |
| Componentes (frontend) | Vitest, Testing Library, MSW | `frontend/src/**/*.test.ts(x)` | cliente HTTP (refresh single-flight), parsing de erros RFC 7807, formatação, esquemas Zod, formulário de login, grelha de espaços e check-in, filtros de tickets → parâmetros |
| End-to-end | Playwright (Chromium desktop + Pixel 7) | `frontend/e2e` | login/logout/sessão, guardas de rotas, check-in → impressão → check-out, gestão de utilizadores, redirecionamentos legados — contra a stack Docker completa |
| Carga | k6 | `tests/load` | `smoke.js` (CI) e `load.js` (staging): leitura da grelha/histórico e tráfego de entradas/saídas com *thresholds* de p95 e taxa de erro |
| ETL legado | MySQL | `infra/mysql/legacy` | validado com um esquema legado de teste (ver [04-operations.md §4](04-operations.md)) |

## 2. Execução

```bash
# Backend
cd backend
dotnet test tests/Parking.UnitTests
dotnet test tests/Parking.IntegrationTests        # requer Docker

# Frontend
cd frontend
npm test
E2E_BASE_URL=https://localhost E2E_ADMIN_USERNAME=admin E2E_ADMIN_PASSWORD=<senha> npm run test:e2e

# Carga (stack em execução)
docker run --rm --network host -v "$PWD/tests/load:/scripts:ro" \
  -e BASE_URL=https://localhost -e ADMIN_USERNAME=admin -e ADMIN_PASSWORD=<senha> -e K6_INSECURE_SKIP_TLS_VERIFY=true \
  grafana/k6:0.55.0 run /scripts/smoke.js
```

Os E2E e os testes de carga fazem muitos logins/refreshes a partir do mesmo IP: no ambiente de teste aumente
`RATE_LIMIT_AUTH_PER_MINUTE`, `RATE_LIMIT_DEFAULT_PER_MINUTE`, `EDGE_AUTH_RATE_PER_SECOND` e `EDGE_API_RATE_PER_SECOND`
(o CI já o faz). Os testes criam os seus próprios dados (marca, espaços, operadores) com nomes únicos.

## 3. Análise estática e formatação

| Área | Ferramenta |
|---|---|
| C# | analisadores .NET (`AnalysisLevel=latest-recommended`), regras de estilo no build (`EnforceCodeStyleInBuild`), `TreatWarningsAsErrors`, `dotnet format`, `.editorconfig` |
| TypeScript | `tsc --strict` (+ `noUncheckedIndexedAccess`), ESLint (typescript-eslint strict, react-hooks, jsx-a11y), Prettier |
| Segurança | CodeQL (C#, TS), Trivy (vulnerabilidades, segredos, IaC, imagens), Dependency Review, `npm audit`, `dotnet list package --vulnerable` |
| Esquema | `dotnet ef migrations has-pending-model-changes` (o modelo não pode divergir das migrations) |

## 4. Resultados da validação (2026-11-07, máquina de desenvolvimento)

| Verificação | Resultado |
|---|---|
| Build backend (Release, analisadores, warnings = erros) + `dotnet format --verify-no-changes` | OK |
| Testes unitários backend | 62/62 |
| Testes de integração/API (MySQL 8.4 + Redis reais) | 29/29 |
| Migrations vs modelo (`has-pending-model-changes`) | sem diferenças |
| Frontend: `tsc`, ESLint (0 avisos), Prettier, build | OK |
| Testes de componentes (Vitest + MSW) | 79/79 |
| E2E Playwright contra a stack Docker (TLS, 2 réplicas) | 10/10 (Chromium + Pixel 7) |
| k6 smoke | 52/52 verificações, 0 % erros, 10 check-ins/outs |
| Load balancing | 15/15 pedidos por réplica; escala 2→3 réplicas detetada sem reload (9/9/9); 40/40 OK ao parar uma réplica |
| Borda | HTTP→HTTPS 301, HTTP/2, HSTS, CSP, X-Frame-Options, nosniff, Referrer/Permissions-Policy; redirects legados 301 |
| Backups | dump + `sha256` verificado |
| ETL legado | migrado, idempotente, inválidos reportados |
| Regras Prometheus (`promtool`) | 5 regras válidas |

**Latência**: com carga leve, leituras em 15–100 ms e login em ~0,4 s (custo intencional do PBKDF2 com 1 CPU por réplica).
Os *thresholds* de latência do k6 **não** foram cumpridos nesta máquina: o próprio macOS estava com *load average* ~20
e a VM do Docker (4 GB) em swap. A validação de desempenho deve correr com `load.js` em staging, com hardware dedicado.

**Problemas encontrados e corrigidos durante a validação**: bloqueio de login com contador inexistente (`null`),
leitura de `DateOnly` pelo provider MySQL, interceptor de *timestamps*, cabeçalhos `RateLimit-*` perdidos em respostas de erro,
*starvation* do *thread pool* quando o coletor OTLP está indisponível (mínimo de threads + alerta), *default server* do Nginx,
dados de teste do k6 que falhavam em silêncio.
