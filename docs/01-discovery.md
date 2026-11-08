# Fase 1 — Discovery: diagnóstico técnico do sistema legado

> Âmbito: análise integral do código PHP original (agora em `legacy/`), antes de qualquer alteração.
> Data da análise: 2026-11-07.

## 1. Visão geral

**Xyami.Park** é um sistema de gestão de parque de estacionamento usado por operadores e administradores:
atribuição de espaços a viaturas (check-in), fecho de tickets (check-out) com cálculo do valor a pagar,
histórico de tickets, gestão de utilizadores e configuração da tarifa.

| Item | Situação encontrada |
|---|---|
| Linguagem / runtime | PHP 8 (sem framework), JavaScript ES Modules no browser |
| Persistência | MySQL via PDO, base `parking` (`root` sem password, `localhost:3306`, *hardcoded*) |
| Esquema / migrações | **Inexistentes no repositório** — esquema inferido a partir das queries (ver §5) |
| Dependências | Nenhuma (Composer vazio). Chart.js e jsPDF referenciados mas **ausentes** do repositório |
| Routing | Router caseiro (`models/routing/index.php`) com mapa `path → include` |
| Front-end | Páginas PHP com HTML + CSS próprio + JS imperativo (`innerHTML`) |
| Testes / CI / Docker | Inexistentes |
| Dimensão | ~84 ficheiros, ~3 000 linhas úteis |

## 2. Arquitetura existente

```
Browser ──► index.php (router) ──► view/*.php            (HTML + guarda de sessão por página)
                              └──► controllers/APIs/*.php ──► models/* (extends DBController extends HttpStatusCode)
                                                                        └──► PDO ──► MySQL
```

- **Modelos** misturam acesso a dados, regras de negócio, serialização JSON e códigos HTTP.
- O estado de pedido é partilhado através de uma propriedade estática global (`DBController::$data`).
- A "autenticação" é uma *cookie* `token` = `base64(id).base64(code).base64(linha completa do utilizador)`,
  criada no browser via `document.cookie`. **Não é assinada.**
- Cada view repete ~20 linhas de descodificação da *cookie* e o menu completo (sem *layout* partilhado).

## 3. Inventário funcional (a preservar)

| # | Funcionalidade | Onde | Perfil |
|---|---|---|---|
| F1 | Login por nome de utilizador + senha; logout | `view/login.php`, `/api/login/request`, `/session-close` | todos |
| F2 | Menus/acesso por perfil: Administrador (`code=2`) vs Operador (`code=1`) | guardas nas views | — |
| F3 | Dashboard: faturação total, espaços livres, nº de entradas, nº de saídas, permanência média | `/dashboard`, `/api/dashboard` | admin |
| F4 | Grelha de espaços com estado (Livre/Ocupado/Indisponível), tempo decorrido, valor acumulado, filtros (matrícula, código, estado, ordem), contagem de resultados, atualização automática a cada 10 s | `/spaces`, `/api/space/get` | todos |
| F5 | Ocupar espaço (check-in): nome, data de nascimento, BI (validado: 14 caracteres, `^\d{9}[A-Z]{2}\d{3}$`), marca → modelo (cascata), cor, matrícula; impressão do ticket | `/api/space/insert/user` | todos |
| F6 | Detalhe do espaço ocupado (cliente, idade, BI, viatura, cor, acumulado) e **fecho do ticket** (check-out) | `/api/space/close` | todos |
| F7 | Tabela de tickets com ordenação por campo e direção | `/tickets`, `/api/ticket/get` | todos |
| F8 | Histórico de tickets com filtros: nome, intervalo de datas, BI, marca, modelo, código do espaço, matrícula, ordenação | `/gestor/tickts-story`, `/api/ticket/get/story` | todos (guarda comentada) |
| F9 | Gestão de utilizadores: listar (com indicador *online*), criar, editar, eliminar | `/gestor/users`, `/api/user/*` | admin |
| F10 | Configuração: renda por minuto, horas grátis, quantidade máxima de espaços | `/settings`, `/api/config/*` | admin |
| F11 | Tema claro/escuro persistido no browser | `public/js/app.js` | todos |
| F12 | Catálogo de marcas e modelos de viaturas (só leitura) | `/api/car/get`, `/api/model/get` | todos |

### Itens sem funcionalidade real (não são funcionalidades a preservar)

| Item | Situação |
|---|---|
| "Tabela de Preços" (`/gestor/table-prices`) | HTML estático com preços por dia da semana **que não são usados em nenhum cálculo** |
| "Promoções" (`/gestor/promotion`) | Página vazia |
| Gráficos semanal/mensal | Dados *hardcoded* (`[65, 59, 80, …]`) e desativados; barras do dashboard decorativas (0–100 %) |
| `/api/login` (mobile) | Aponta para ficheiro inexistente; `mobile/auth/login.php` devolve `"hello world"` |
| `space/new`, `space/car/new`, `space/filter` | *Stubs* vazios / código comentado |

## 4. Problemas encontrados

### 4.1 Segurança (OWASP Top 10 / API Security Top 10) — **críticos**

| # | Problema | Evidência | Impacto |
|---|---|---|---|
| S1 | **SQL Injection generalizada** — valores do pedido interpolados em SQL | `auth`, `User::*`, `Ticket::*`, `Space::*`, `Configuration`, `ORDER BY` vindo do cliente (`filter`, `order_by`, `order`) | Leitura/alteração total da base de dados |
| S2 | **Token forjável** — *cookie* sem assinatura; basta `base64` de `id`/`code` de um admin existente | `controllers/isAuth.php`, views | Escalada para administrador |
| S3 | **APIs sem autenticação** — única "proteção" é a ausência do cabeçalho `Sec-Fetch-User` | todos os `controllers/APIs/*` | Qualquer pessoa cria administradores, apaga utilizadores, fecha tickets |
| S4 | **Hashes de senha expostos** — `/api/user/data` devolve `senha`; o token na *cookie* contém a linha completa (incl. hash) | `User::getData`, `verifyExistence` | Fuga de credenciais |
| S5 | **MD5 sem salt** para senhas | `md5($password)` | Senhas triviais de quebrar |
| S6 | **Operações destrutivas via GET** (`/api/user/delete?id=`, `/api/space/close?id=`) e sem proteção CSRF | services JS | CSRF trivial |
| S7 | **XSS armazenado** — dados de clientes/utilizadores inseridos via `innerHTML` e `document.write` | `space.js`, `ticket.js`, `story.js`, `user.js` | Execução de script na sessão de um admin |
| S8 | Credenciais da BD *hardcoded* (`root` / senha vazia) | `DBcontroller.php` | Privilégio máximo na BD |
| S9 | Sem *rate limiting* nem bloqueio por tentativas | login | Força bruta |
| S10 | Mensagens de exceção PDO devolvidas ao cliente | `insert/update/delete` | Divulgação de informação |
| S11 | *Cookie* sem `HttpOnly`/`Secure`/`SameSite`; expiração calculada com `getDay()` (dia da semana) | `login.js` | Roubo de sessão via XSS |
| S12 | CORS fixo em `http://127.0.0.1:5173` | `index.php` | Configuração não portável |

### 4.2 Erros funcionais e de regra de negócio

| # | Problema | Consequência |
|---|---|---|
| B1 | Ao fechar o ticket grava-se `montante = minutos × 10` (tarifa fixa, ignora horas grátis e a tarifa configurada) | Valor gravado ≠ valor mostrado ao operador |
| B2 | Três fórmulas de preço diferentes (grelha usa `LAST_DAY` do mês para converter meses em dias; histórico usa `TIMESTAMPDIFF`; fecho usa ×10) | Totais inconsistentes entre ecrãs |
| B3 | O fecho faz `UPDATE consumidores SET data_hora_saida = NOW() WHERE bi = ?` | **Reescreve a hora de saída de todas as visitas anteriores** do mesmo cliente |
| B4 | O fecho escolhe "a última linha" de um `LEFT JOIN` por BI + histórico | Pode usar a data de entrada errada → valor errado |
| B5 | `GetAllTicket` coloca `;` antes do `ORDER BY` e só ordena se `$filter` (sempre vazio) | Ordenação da página Tickets nunca funciona |
| B6 | Filtro por código no histórico gera `LIKE = '%…%'` (SQL inválido) | Erro ao filtrar por espaço |
| B7 | Histórico limitado por `LIMIT quant_max_espaco` (nº de espaços!) e sem paginação | Histórico truncado |
| B8 | Dashboard: o 5.º cartão mostra a permanência média com rótulo errado; o seletor do 6.º cartão aponta para um elemento inexistente; `maxEarnToday` não é "de hoje" e usa tarifa fixa | Métricas enganadoras |
| B9 | Editar um utilizador exige a senha atual **desse** utilizador | O admin não consegue repor senhas |
| B10 | Unicidade do nome de utilizador só validada na criação | Duplicados via edição |
| B11 | `space()` cria um novo `setInterval` a cada pesquisa/atualização; `user.js` re-associa *handlers* a cada 500 ms | Fugas de *timers*, pedidos duplicados |
| B12 | Sem controlo de concorrência: dois operadores podem ocupar o mesmo espaço / o mesmo BI em simultâneo | Dupla ocupação |
| B13 | Fuso horário `Africa/Lagos` no PHP vs `NOW()` do servidor MySQL | Durações inconsistentes |

### 4.3 Qualidade de código e manutenção

- Sem separação de camadas; modelos devolvem JSON e definem códigos HTTP.
- Duplicação massiva: guarda de sessão e menu copiados em 9 views; `FNtoast` copiado em 4 ficheiros;
  renderização da grelha de espaços duplicada dentro de `space.js`.
- Estado global estático (`DBController::$data`) partilhado entre métodos.
- Sem validação centralizada, sem tipagem, sem testes, sem *linting*.
- Código comentado extenso e um comentário HTML no fim de `view/operer/users.php` que é servido ao browser.
- Assets referenciados mas inexistentes (`public/libs/chartjs`, `public/libs/jsPDF-1.3.2`).

### 4.4 Performance e escalabilidade

- Sem paginação; sem índices conhecidos; `SELECT *`; consultas de configuração repetidas em cada pedido.
- *Polling* de 10 s que refaz também os pedidos de marcas.
- Uma ligação PDO nova por instância de modelo (algumas rotas abrem várias).
- Sessão baseada em *cookie* não assinada impede escalar com segurança; nenhuma cache.

## 5. Esquema legado inferido (base `parking`)

| Tabela | Colunas (inferidas) | Notas |
|---|---|---|
| `employee` | `id`, `code` (1=operador, 2=admin), `nome`, `nome_completo`, `senha` (MD5), `active` (bool, "online") | ordem de colunas confirmada pelo `INSERT … VALUES(default, …)` |
| `config` | `renda_min`, `num_hora_gratis`, `quant_max_espaco` | linha única |
| `espacos` | `id`, `nome` (ex. `A1`), `codigo` (`uniqid`), `ativo` (`s`/…), `estado` (`a` livre, `i` ocupado, `m` indisponível), `bi` (cliente atual) | criados por script A1…Z10 |
| `consumidores` | `id`, `nome`, `bi`, `idade` (data de nascimento), `valor`, `id_marca_carro`, `id_modelo_carro`, `cor_carro`, `matricula_carro`, `data_hora_entrada`, `data_hora_saida`, `operador`, `reservado`, `renda_min` | **uma linha por visita** |
| `ticket_historico` | `id`, `id_consumidor`, `id_espaco`, `montante`, `marca`, `modelo`, `cor`, `matricula`, `data_entrada`, `data_saida`, `renda_min`, `operador` | criado no fecho |
| `marcas` | `id`, `nome` | dados de referência |
| `modelos` | `id`, `id_marca`, `nome` | dados de referência |

## 6. Conclusão

O sistema cumpre um fluxo de negócio claro e pequeno, mas **não é recuperável incrementalmente**: as falhas
de autenticação (S2–S4) e de SQL Injection (S1) atravessam todas as camadas, e a lógica de faturação (B1–B4)
produz valores inconsistentes. A recomendação é uma **reescrita com paridade funcional** (F1–F12), com
migração de dados do esquema legado (§5) para o novo esquema, conforme o plano em
[02-architecture.md](02-architecture.md).
