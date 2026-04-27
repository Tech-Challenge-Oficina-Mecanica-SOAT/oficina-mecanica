README - Como rodar a aplicação

Visão geral

Este repositório contém uma API ASP.NET (.NET 10) com EF Core e um serviço PostgreSQL via docker-compose. O arquivo docker-compose usado está em: src/OficinaMecanica.API/docker-compose.yaml. Há também um Dockerfile multi-stage em src/OficinaMecanica.API/Dockerfile.

Pré-requisitos

- .NET 10 SDK
- Docker e Docker Compose
- (Opcional) psql ou cliente GUI (DBeaver/pgAdmin)

Rodando com Docker (recomendado)

1) Subir Postgres + API (build automático):

   docker compose -f src/OficinaMecanica.API/docker-compose.yaml up -d --build

2) Verificar status dos containers:

   docker compose -f src/OficinaMecanica.API/docker-compose.yaml ps
   docker inspect --format '{{.State.Health.Status}}' oficina_postgres

3) Ver logs:

   docker logs -f oficina_postgres
   docker logs -f oficina_api

A API estará acessível em http://localhost:5165/ e a documentação interativa (Scalar) em http://localhost:5165/scalar. O Dockerfile expõe a porta 5000.

Conectar ao banco (psql)

- Abrir shell psql dentro do container:

  docker exec -it oficina_postgres psql -U postgres -d OficinaDB

- Comandos úteis:
  - Listar tabelas: \dt
  - Ver migrations aplicadas: SELECT * FROM "__EFMigrationsHistory";

EF Core / Migrations (local ou em CI)

1) Instalar o EF CLI (se necessário):

   # Global
   dotnet tool install --global dotnet-ef

   # Ou local (repo)
   dotnet new tool-manifest --force
   dotnet tool install dotnet-ef

2) Adicionar pacote Design no projeto Infrastructure (se ainda não):

   dotnet add src/OficinaMecanica.Infrastructure package Microsoft.EntityFrameworkCore.Design

3) Gerar migration (recomendado especificar projects):

   dotnet ef migrations add InitialCreate --project src/OficinaMecanica.Infrastructure --startup-project src/OficinaMecanica.API --output-dir Migrations

4) Aplicar migrations (criar tabelas):

   dotnet ef database update --project src/OficinaMecanica.Infrastructure --startup-project src/OficinaMecanica.API

Observações importantes

- A connection string usada pela API via Docker Compose é passada por variável de ambiente: ConnectionStrings__DefaultConnection (ex.: Host=postgres;Port=5432;Database=OficinaDB;Username=postgres;Password=SuaSenha).
- Em container, use o hostname do serviço (postgres) em vez de localhost.
- Não deixe senhas em texto em produção; use secrets/variáveis de ambiente ou secret manager.
- Recomenda-se usar Database.Migrate() ou aplicar migrations via pipeline em vez de EnsureCreated() em produção.
- Se o comando `dotnet ef` não for encontrado, confirme que o diretório de ferramentas do dotnet está no PATH (ex.: %USERPROFILE%\.dotnet\tools no Windows).

Dicas de troubleshooting

- Se a API não conectar ao DB, verifique se o container Postgres está STARTED e HEALTHY antes de iniciar a API.
- Para evitar falhas de startup por dependência, adicione retry na configuração do DbContext:
  options.UseNpgsql(conn, o => o.EnableRetryOnFailure(...));

Se precisar, eu posso adicionar um script de espera (wait-for) ou aplicar Database.Migrate() automaticamente no startup.

## Autenticação

A API utiliza **JWT Bearer Token** para autenticação. O fluxo é:

### Endpoint de login

```
POST /auth/login
Content-Type: application/json

{
  "email": "usuario@email.com",
  "senha": "senha123"
}
```

Resposta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiracao": "2025-01-01T12:05:00Z"
}
```

### Como usar o token

Inclua o token em todas as requisições autenticadas via header:

```
Authorization: Bearer {token}
```

O token expira em **5 minutos**. Faça um novo login para obter um novo token.

### Perfis de acesso

| Perfil  | Descrição |
|---------|-----------|
| `Admin` | Acesso total a todas as rotas administrativas |
| `Cliente` | Acesso restrito a consultas do próprio cliente |

### Rotas públicas (sem autenticação)

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/auth/login` | Autenticar e obter token |
| `POST` | `/auth/registrar` | Registrar novo usuário (uso interno) |
| `GET`  | `/publico/os/{id}/status` | Consultar status de uma OS sem autenticação |

### Rotas protegidas

Todas as demais rotas exigem `Authorization: Bearer {token}` com perfil `Admin`:

| Método | Rota |
|--------|------|
| `GET/POST/PUT/DELETE` | `/clientes` |
| `GET/POST/PUT/DELETE` | `/veiculos` |
| `GET/POST/PUT/DELETE` | `/servicos` |
| `GET/POST/PUT/DELETE` | `/pecas` |
| `GET/POST/PUT/DELETE` | `/ordens-servico` |
| `GET/POST/PUT` | `/aprovacoes` |

### Teste rápido do fluxo

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@oficina.com","senha":"Admin@123"}' \
  | jq -r '.token')

# 2. Rota protegida com token
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/clientes

# 3. Rota pública sem token
curl http://localhost:5000/publico/os/1/status
```

## Como rodar o scan de qualidade (SonarQube)

### 1. Subir o SonarQube

O SonarQube está incluído no `docker-compose.yaml`. Para subí-lo:

```bash
docker compose up -d sonarqube
```

Aguarde ~1 minuto e acesse `http://localhost:9000`. Login padrão: **admin / admin** (será solicitada troca na primeira vez).

### 2. Criar o projeto e gerar o token

1. Em `http://localhost:9000`, crie um projeto com a chave `mecanica-api`
2. Vá em **My Account → Security → Generate Token** e copie o token gerado

### 3. Instalar o scanner (apenas uma vez)

```bash
dotnet tool install --global dotnet-sonarscanner
```

### 4. Executar o scan

Na raiz do repositório, substituindo `SEU_TOKEN`:

```bash
dotnet sonarscanner begin /k:"mecanica-api" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="SEU_TOKEN"
dotnet build OficinaMecanica.slnx
dotnet sonarscanner end /d:sonar.token="SEU_TOKEN"
```

### 5. Executar com cobertura de testes

```bash
dotnet sonarscanner begin /k:"mecanica-api" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.token="SEU_TOKEN" \
  /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

dotnet build OficinaMecanica.slnx
dotnet test --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
dotnet sonarscanner end /d:sonar.token="SEU_TOKEN"
```

Resultados em: `http://localhost:9000/dashboard?id=mecanica-api`

## Cobertura de Testes

**Como gerar relatório de cobertura:**

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:Html
```

Abra `TestResults/CoverageReport/index.html` no navegador.