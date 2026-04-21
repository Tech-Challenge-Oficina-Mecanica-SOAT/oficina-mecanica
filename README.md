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

A API estará acessível em http://localhost:5000/ e a UI Scalar em http://localhost:5000/scalar (se habilitada). O Dockerfile expõe a porta 5000.

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