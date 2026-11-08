# Segurança

## 1. Correções face ao sistema legado

| Legado ([01-discovery.md §4.1](01-discovery.md)) | Novo sistema |
|---|---|
| S1 SQL Injection generalizada | EF Core com parâmetros em todas as consultas; o único SQL manual (`ReportingQueries`) usa `FormattableString` parametrizado; ordenação por *whitelist* (`TicketSortBy`) |
| S2 Token forjável em *cookie* | JWT HS256 assinado (15 min) validado em emissor, audiência, algoritmo, assinatura e validade |
| S3 APIs sem autenticação | Política *fallback* exige autenticação em **todos** os endpoints; exceções explícitas (`login`, `refresh`, `logout`, `health`, OpenAPI) |
| S4 Hashes expostos | Nenhum DTO expõe hashes; o access token transporta apenas id, nome e papel |
| S5 MD5 sem salt | PBKDF2-HMAC-SHA512, 210 000 iterações, salt aleatório; MD5 legado aceite **uma vez** e convertido |
| S6 GET destrutivos / CSRF | Verbos corretos (POST/PUT/PATCH/DELETE); API autenticada por cabeçalho `Authorization` (imune a CSRF); endpoints de cookie (`refresh`/`logout`) exigem `X-Requested-With` + `SameSite=Strict` |
| S7 XSS | React escapa todo o conteúdo; sem `dangerouslySetInnerHTML`; CSP `script-src 'self'`; impressão por rota própria (sem `document.write`) |
| S8 Credenciais *hardcoded* | Segredos só por variáveis de ambiente (`.env` fora do git, `.env.example` sem valores); validação no arranque (`ValidateOnStart`) |
| S9 Sem *rate limiting* | Limite por IP no Nginx + limites distribuídos por utilizador/IP em Redis + bloqueio por nome de utilizador após 5 falhas (15 min) |
| S10 Exceções expostas | `GlobalExceptionHandler`: erros 5xx com mensagem genérica + `correlationId`; detalhes apenas nos logs |
| S11 Cookie inseguro | Refresh token em cookie `HttpOnly; Secure; SameSite=Strict; Path=/api/v1/auth`; access token só em memória no browser |
| S12 CORS fixo | Mesma origem por omissão (SPA servida atrás do mesmo Nginx); CORS configurável por *allowlist* (`Cors__AllowedOrigins`) |

## 2. Controlos (OWASP Top 10 2021 / API Security Top 10 2023)

| Categoria | Controlos |
|---|---|
| A01 Broken Access Control / API1, API5 | RBAC com permissões granulares por endpoint; *fallback policy*; operador não acede a gestão; regras anti-*lockout* (não se desativar/despromover a si próprio nem o último admin) |
| A02 Cryptographic Failures | TLS 1.2/1.3 no Nginx, HSTS (2 anos); PBKDF2-SHA512; refresh tokens aleatórios de 512 bits, guardados apenas como SHA-256 |
| A03 Injection | Consultas parametrizadas; validação FluentValidation (servidor) + Zod (cliente) com regras equivalentes |
| A04 Insecure Design | Invariantes na base de dados (índices únicos em colunas geradas) contra dupla ocupação; idempotência em check-in/out; *tariff snapshot* |
| A05 Security Misconfiguration | Contentores não-root, `read_only`, `cap_drop: ALL`, `no-new-privileges`; rede `backend` interna; `server_tokens off`; OpenAPI desativado em produção; cabeçalhos CSP/X-Frame-Options/nosniff/Referrer-Policy/Permissions-Policy/COOP |
| A06 Vulnerable Components | Versões fixadas; `dotnet list package --vulnerable`, `npm audit`, Trivy (fs + imagem), Dependency Review, CodeQL no CI |
| A07 Identification & Auth Failures | *Lockout* por utilizador; mensagem genérica de credenciais; verificação com tempo uniforme para utilizadores inexistentes; rotação de refresh token com deteção de reutilização (revoga a família); revogação de sessões ao alterar/redefinir senha ou desativar a conta |
| A08 Software & Data Integrity | Imagens com SBOM e proveniência; deploy só por pipeline com aprovação em produção |
| A09 Logging & Monitoring Failures | Auditoria persistente de operações sensíveis (logins, utilizadores, tarifa, espaços, tickets); logs estruturados com `CorrelationId`; alertas Prometheus (erros, latência, picos de logins falhados) |
| A10 SSRF | A API não faz pedidos a URLs fornecidos pelo utilizador |
| API4 Unrestricted Resource Consumption | Paginação obrigatória (máx. 100); corpo máx. 1 MB (Kestrel e Nginx); limites de pedidos; *timeouts* no proxy |
| API3/API6 Mass assignment / business flows | Corpos de pedido dedicados (records) — nunca entidades; fluxos de negócio validados no domínio |

## 3. Menor privilégio

* **MySQL**: `parking` (dono do esquema) só no `migrator`; a API usa `parking_app` com `SELECT/INSERT/UPDATE/DELETE`;
  os backups usam `parking_backup` (só leitura).
* **Redis**: protegido por senha, numa rede Docker interna sem portas publicadas em produção.
* **Contentores**: utilizadores não-root (`app`, `nginx`), sistema de ficheiros só de leitura onde possível.
* **Aplicação**: permissões por papel (`Operator` vs `Admin`) — ver [02-architecture.md §5](02-architecture.md).

## 4. Auditoria

Tabela `audit_logs` (consultável em *Gestão → Auditoria*): instante, utilizador, ação, entidade, IP, `correlationId`
e detalhes JSON. Ações registadas: `auth.login.succeeded|failed`, `auth.logout`, `auth.refresh.reuse_detected`,
`auth.password_changed`, `user.created|updated|password_reset|activated|deactivated`, `settings.updated`,
`space.created|bulk_created|status_changed|deleted`, `ticket.opened|closed`, `catalog.brand_created|model_created`,
`system.bootstrap_admin_created`, `system.legacy_import`.

## 5. Notas operacionais

* A chave `JWT_SIGNING_KEY` é partilhada pelas réplicas; rotação descrita em [04-operations.md §7](04-operations.md).
* A API não usa ASP.NET Core Data Protection para dados persistentes (as chaves efémeras por réplica são inofensivas);
  se no futuro for usado (ex.: cookies de autenticação), persistir as chaves num repositório partilhado.
* O certificado de desenvolvimento é auto-assinado; em produção usar certificados de uma CA (ex.: Let's Encrypt).
