# Xyami.Park — Frontend

SPA em **React 19 + TypeScript** que substitui as páginas PHP do sistema legado. Consome a API
`/api/v1` descrita em [`docs/02-architecture.md`](../docs/02-architecture.md) §6.

## Stack

| Área               | Tecnologia                                                                          |
| ------------------ | ----------------------------------------------------------------------------------- |
| Build / dev server | Vite 8 (`@vitejs/plugin-react`)                                                     |
| Linguagem          | TypeScript 5.9 (`strict`, `noUncheckedIndexedAccess`)                               |
| Rotas              | TanStack Router (file-based, `autoCodeSplitting`)                                   |
| Estado do servidor | TanStack Query 5                                                                    |
| Tabelas            | TanStack Table 8 (paginação e ordenação no servidor)                                |
| UI                 | Tailwind CSS 4, shadcn/ui (Radix), lucide-react, `motion` (Framer Motion), Recharts |
| Formulários        | React Hook Form + Zod 4 (`@hookform/resolvers`)                                     |
| HTTP               | axios (refresh de sessão single-flight)                                             |
| Testes             | Vitest + Testing Library + MSW; Playwright (E2E)                                    |
| Qualidade          | ESLint 9 (typescript-eslint strict, react-hooks, jsx-a11y), Prettier                |

## Scripts

| Comando                           | Descrição                                                                                                                                                      |
| --------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `npm run dev`                     | Servidor de desenvolvimento em `http://localhost:5173`; `/api` e `/health` são encaminhados para `VITE_API_PROXY_TARGET` (por omissão `http://localhost:8080`) |
| `npm run build`                   | `tsc --noEmit` + build de produção em `dist/`                                                                                                                  |
| `npm run preview`                 | Serve o build localmente                                                                                                                                       |
| `npm run typecheck`               | Verificação de tipos                                                                                                                                           |
| `npm run lint`                    | ESLint, sem avisos tolerados                                                                                                                                   |
| `npm run format` / `format:check` | Prettier (inclui ordenação de classes Tailwind)                                                                                                                |
| `npm test` / `test:watch`         | Testes unitários e de componentes (Vitest + MSW)                                                                                                               |
| `npm run test:e2e`                | Playwright contra uma stack em execução (ver abaixo)                                                                                                           |

Requer Node.js ≥ 22.12 (a imagem Docker usa Node 24 LTS).

## Estrutura

```
src/
  main.tsx                 arranque: tema, QueryClient, router, providers
  app/                     QueryClient (erros globais), router (contexto, defaults), prefetch
  routes/                  rotas file-based — finas: validação de search params, guardas, loaders
  routeTree.gen.ts         gerado pelo plugin do TanStack Router (versionado; não editar)
  features/<módulo>/       api.ts (query keys, queryOptions, mutations), types.ts, schemas.ts,
                           search.ts (search params), components/, pages/
  components/ui/           design system (shadcn/ui)
  components/common/       DataTable, Pagination, FormField, ConfirmDialog, estados de erro/vazio
  components/layout/       AppShell, sidebar (filtrada por permissões), header, tema, menu do utilizador
  lib/                     http, sessão em memória, ApiError, formatação, intervalos de datas, relógio
  hooks/                   useDebouncedValue, useIsMobile, useTheme
tests/                     setup do Vitest, servidor MSW, fixtures
e2e/                       Playwright
```

Módulos: `auth`, `dashboard`, `spaces`, `tickets`, `customers`, `catalog`, `settings`, `users`, `audit`.

## Rotas

| Rota                       | Permissão         | Conteúdo                                                                    |
| -------------------------- | ----------------- | --------------------------------------------------------------------------- |
| `/login`                   | —                 | Início de sessão                                                            |
| `/`                        | `dashboard.view`  | KPIs por período e gráficos de receita/entradas/saídas                      |
| `/spaces`                  | `spaces.view`     | Grelha em tempo real, check-in (com pesquisa de cliente por BI) e check-out |
| `/tickets`                 | `tickets.view`    | Abas Ativos/Histórico, filtros avançados e predefinições de datas           |
| `/tickets/$ticketId/print` | `tickets.view`    | Ticket imprimível                                                           |
| `/tariff`                  | `settings.view`   | Tarifa em vigor e exemplos calculados pelo servidor                         |
| `/management/users`        | `users.manage`    | Utilizadores: criar, editar, redefinir senha, desativar/reativar            |
| `/management/settings`     | `settings.manage` | Tarifa, espaços (individual/lote/manutenção) e catálogo de marcas/modelos   |
| `/management/audit`        | `audit.view`      | Registo de auditoria                                                        |
| `/profile`                 | sessão            | Alterar a própria senha                                                     |

Operadores entram em `/spaces`; administradores em `/`. Rotas sem permissão redirecionam para a
primeira página permitida. Os URLs do sistema legado são redirecionados (301) pelo `nginx.conf`.

## Convenções

- **Sessão**: o access token fica **só em memória** (`lib/session.ts`); o refresh token é um cookie
  `HttpOnly` gerido pela API. No arranque, `/auth/refresh` restaura a sessão. Um `401` dispara um único
  refresh partilhado por todos os pedidos pendentes e repete o pedido uma vez; se falhar, a sessão é
  limpa e o utilizador volta ao login com `?redirect=` (apenas caminhos internos são aceites).
- **Erros**: todas as respostas de erro são convertidas em `ApiError` (RFC 7807). Erros de validação
  do servidor são associados aos campos do formulário (`applyFieldErrors`); os restantes aparecem como
  alerta do formulário ou _toast_. Erros 4xx não são repetidos automaticamente.
- **Estado na URL**: filtros, paginação, ordenação e abas vivem nos _search params_ (validados com Zod,
  valores inválidos são descartados), por isso as vistas são partilháveis e o botão "voltar" funciona.
- **Dados**: cada módulo expõe _query key factories_ e `queryOptions`; mutações invalidam as chaves
  afetadas (ex.: check-in → espaços, tickets e dashboard). A grelha de espaços atualiza a cada 10 s e os
  temporizadores avançam localmente a cada segundo, corrigidos pelo relógio do servidor (`serverTime`).
- **Idempotência**: check-in e check-out enviam `Idempotency-Key` (UUID por submissão).
- **Acessibilidade**: rótulos associados a todos os campos, erros ligados por `aria-describedby`,
  estado dos espaços em texto + ícone (não só cor), foco visível, _skip link_, tabelas com `caption` e
  `aria-sort`, animações desativadas com `prefers-reduced-motion`.
- **Sem dados fictícios**: o código da aplicação só mostra dados da API; o MSW é usado apenas nos testes.

## Testes E2E

Correm contra uma stack real (por exemplo `docker compose up` na raiz):

```bash
E2E_BASE_URL=https://localhost \
E2E_ADMIN_USERNAME=admin E2E_ADMIN_PASSWORD='…' \
npx playwright install --with-deps chromium && npm run test:e2e
```

Os testes criam dados próprios com sufixos únicos (uma marca, um espaço `E####` quando há capacidade,
um operador) e cobrem: login/logout e restauro da sessão, redirecionamentos legados, fluxo completo de
check-in → impressão → check-out → histórico, gestão de utilizadores e controlo de acesso por perfil.

## Container

`Dockerfile` (multi-stage): Node 24 compila o bundle → `nginxinc/nginx-unprivileged` serve `dist/` na
porta **8080** como utilizador não-root, com _fallback_ SPA, cache longa para `/assets/*`, `no-cache`
para `index.html`, gzip, `/healthz` e redirecionamentos dos URLs legados. Cabeçalhos de segurança
(CSP, HSTS, etc.) e o proxy de `/api` são responsabilidade do proxy de entrada.

```bash
docker build -t xyami-park-web .
docker run --rm -p 8081:8080 xyami-park-web
```
