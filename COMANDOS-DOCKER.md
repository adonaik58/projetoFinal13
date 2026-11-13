# Guia de Comandos Docker & C# (.NET 10) — Xyami.Park

Este documento reúne todos os comandos necessários para compilar, gerir, aplicar migrações e testar o backend C# (.NET 10) do **Xyami.Park** rodando em contentores **Docker**.

---

## 1. Arranque Rápido & Aplicação da Migration de Marcas/Modelos

Ao atualizar o código C# ou adicionar novas migrations (como a `20261110000000_VehicleCatalogSeed`), o Docker compila a aplicação e o container **`migrator`** executa automaticamente `dotnet Parking.Api.dll --migrate`.

### Apenas Backend (Migrator + API)
```bash
docker compose up -d --build migrator api
```
* **O que faz:**
  * Compila a solução C# no estágio multi-stage do `backend/Dockerfile` (`.NET 10 SDK`).
  * Inicia e aguarda o healthcheck de MySQL e Redis.
  * Executa o container `migrator` que aplica as migrações pendentes no MySQL.
  * Reinicia os containers da `api` com o novo código.

### Apenas Frontend (Web)
```bash
docker compose up -d --build web
```
* **O que faz:** Reconstrói o bundle estático do React SPA com o Vite e atualiza o container web.

### Toda a Stack (Frontend + Backend + Banco + Cache + Nginx)
```bash
# 1. Se for a primeira vez no ambiente de desenvolvimento, gere os certificados TLS:
./infra/scripts/generate-dev-certs.sh

# 2. Suba todos os serviços
docker compose up -d --build --wait
```

---

## 2. Inspeção e Acompanhamento de Logs

### Ver se a migração rodou com sucesso
```bash
docker compose logs migrator
```
* **O que esperar:** Mensagem indicando que as migrações foram aplicadas e `Database initialization completed` com código de saída 0 (`exited with code 0`).

### Ver logs da API em tempo real
```bash
docker compose logs -f api
```
* **O que faz:** Exibe os logs estruturados emitidos pelo Serilog (pedidos HTTP, autenticação, requisições lentas, etc.).

### Forçar a execução da migração manualmente
```bash
docker compose run --rm migrator
```
* **O que faz:** Cria uma instância avulsa do container migrator para reaplicar ou checar o status das migrações sem alterar os serviços em execução.

---

## 3. Comandos .NET / C# usando Docker SDK (Sem instalar .NET localmente)

Você não precisa do SDK .NET instalado na sua máquina host. É possível utilizar a imagem oficial do .NET 10 SDK montando o diretório `backend`:

### A. Compilar a Solução C#
```bash
docker run --rm \
  -v $(pwd)/backend:/src \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0-alpine \
  dotnet build -c Release
```
* **O que faz:** Valida a compilação de todos os projetos (`Domain`, `Application`, `Infrastructure`, `Api`) e aponta erros de sintaxe ou referências.

### B. Executar os Testes Unitários (xUnit)
```bash
docker run --rm \
  -v $(pwd)/backend:/src \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0-alpine \
  dotnet test tests/Parking.UnitTests/Parking.UnitTests.csproj
```
* **O que faz:** Corre todos os testes unitários isolados (validação FluentValidation, regras de domínio, dispatcher e cálculo de tarifas).

### C. Listar Migrações do EF Core
```bash
docker run --rm \
  -v $(pwd)/backend:/src \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0-alpine \
  sh -c "dotnet tool install --global dotnet-ef && export PATH=\"\$PATH:/root/.dotnet/tools\" && dotnet ef migrations list --project src/Parking.Infrastructure --startup-project src/Parking.Api"
```
* **O que faz:** Instala a ferramenta CLI `dotnet-ef` no container e lista todas as migrations registradas no projeto.

---

## 4. Consultar a Base de Dados MySQL via Docker

Para validar que as marcas e modelos foram inseridos diretamente na base de dados `parking`:

### Total de Marcas Inseridas
```bash
docker compose exec mysql mysql -u parking -p${MYSQL_PASSWORD:-parking_dev_password} parking -e "SELECT COUNT(*) AS total_marcas FROM vehicle_brands;"
```

### Total de Modelos Inseridos
```bash
docker compose exec mysql mysql -u parking -p${MYSQL_PASSWORD:-parking_dev_password} parking -e "SELECT COUNT(*) AS total_modelos FROM vehicle_models;"
```

### Amostra de Marcas com Quantidade de Modelos
```bash
docker compose exec mysql mysql -u parking -p${MYSQL_PASSWORD:-parking_dev_password} parking -e "SELECT b.name AS marca, COUNT(m.id) AS modelos FROM vehicle_brands b LEFT JOIN vehicle_models m ON b.id = m.brand_id GROUP BY b.name ORDER BY modelos DESC LIMIT 15;"
```

---

## 5. Testar os Endpoints da API via cURL

### Passo 1: Obter o Token JWT
```bash
TOKEN=$(curl -k -s -X POST https://localhost/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin12345"}' \
  | jq -r '.accessToken')

echo "Token: $TOKEN"
```

### Passo 2: Listar as Marcas
```bash
curl -k -s -X GET https://localhost/api/v1/catalog/brands \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.[0:10]'
```

### Passo 3: Listar Modelos de uma Marca (Ex: ID 1 - Toyota)
```bash
curl -k -s -X GET https://localhost/api/v1/catalog/brands/1/models \
  -H "Authorization: Bearer $TOKEN" \
  | jq .
```

---

## 6. Limpeza e Reset (Manutenção)

### Limpar Cache do Redis (Catálogo)
O catálogo de marcas e modelos fica armazenado em cache por 6 horas. Se precisar forçar a atualização imediata:
```bash
docker compose exec redis sh -c "REDISCLI_AUTH=\"\$REDIS_PASSWORD\" redis-cli flushdb"
```

### Reset Completo do Ambiente (Clean State)
> [!WARNING]
> Este comando remove os volumes persistentes (`mysql-data`). Todos os dados são apagados e o banco será recriado do zero no próximo arranque.

```bash
# 1. Parar containers e remover volumes
docker compose down -v

# 2. Reconstruir e subir tudo novamente
docker compose up -d --build
```

### Acessar a Shell de um Container
```bash
# Shell da API (.NET runtime)
docker compose exec api sh

# Shell do MySQL
docker compose exec mysql bash

# CLI do Redis
docker compose exec redis sh -c "REDISCLI_AUTH=\"\$REDIS_PASSWORD\" redis-cli"
```

