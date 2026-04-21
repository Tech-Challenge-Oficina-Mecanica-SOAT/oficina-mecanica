# README - Banco de Dados (Postgres)

Arquivo com comandos e instruções rápidas para subir o Postgres via Docker, gerar migrations e verificar tabelas.

## Subir o Postgres com Docker Compose

No terminal, a partir da raiz do repositório:

```bash
docker compose -f src/OficinaMecanica.API/docker-compose.yaml up -d
```

Para listar containers gerenciados por esse compose:

```bash
docker compose -f src/OficinaMecanica.API/docker-compose.yaml ps
```

Ver logs / health do container:

```bash
docker logs -f oficina_postgres
docker inspect --format '{{.State.Health.Status}}' oficina_postgres
```

> Observação: se já existir um Postgres local usando a porta 5432, ajuste o mapeamento de portas no docker-compose (ex.: "5433:5432") e atualize a connection string em appsettings.json.

## Conectar ao banco (psql)

Entrar no shell psql dentro do container:

```bash
docker exec -it oficina_postgres psql -U postgres -d OficinaDB
```

Comandos úteis no psql:

- Listar tabelas: `\dt`
- Ver migrations aplicadas: `SELECT * FROM "__EFMigrationsHistory";`

One-liners:

```bash
docker exec -it oficina_postgres psql -U postgres -d OficinaDB -c "\dt"
docker exec -it oficina_postgres psql -U postgres -d OficinaDB -c 'SELECT * FROM "__EFMigrationsHistory";'
```

Também é possível conectar com GUI (DBeaver / pgAdmin / DataGrip):
- Host: localhost
- Porta: 5432 (ou a porta mapeada)
- Database: OficinaDB
- Usuário: postgres
- Senha: SuaSenha

## EF Core - Preparação

Instalar pacote de design no projeto Infrastructure:

```bash
dotnet add src/OficinaMecanica.Infrastructure package Microsoft.EntityFrameworkCore.Design
```

Se houver conflito de versões do EF Core, alinhe para a mesma versão (ex.: 10.0.6).

Comandos sugeridos para alinhar versões (execute conforme necessário):

```bash
# No projeto Infrastructure
dotnet add src/OficinaMecanica.Infrastructure package Microsoft.EntityFrameworkCore --version 10.0.6
dotnet add src/OficinaMecanica.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 10.0.6
dotnet add src/OficinaMecanica.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.6

# No projeto API (se necessário)
dotnet add src/OficinaMecanica.API package Microsoft.EntityFrameworkCore --version 10.0.6
dotnet add src/OficinaMecanica.API package Microsoft.EntityFrameworkCore.Design --version 10.0.6
dotnet add src/OficinaMecanica.API package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.6
```

## Migrations

Gerar migration (exemplo):

```bash
dotnet ef migrations add InitialCreate --project src/OficinaMecanica.Infrastructure --startup-project src/OficinaMecanica.API --output-dir Migrations
```

Aplicar migrations ao banco (criar tabelas):

```bash
dotnet ef database update --project src/OficinaMecanica.Infrastructure --startup-project src/OficinaMecanica.API
```

Observações:
- `dotnet ef migrations add` gera a migration, mas não cria as tabelas no banco. Use `dotnet ef database update` para aplicar.
- Se o comando `dotnet ef` não estiver disponível, instale a ferramenta:
  - Global: `dotnet tool install --global dotnet-ef` (feche/abra terminal após instalação)
  - Local: `dotnet new tool-manifest --force` && `dotnet tool install dotnet-ef`

## Design-time DbContextFactory (recomendado)

Para evitar que o EF Tools tente executar o startup da API (e falhe por falta de DB), crie um `DesignTimeDbContextFactory` no projeto `OficinaMecanica.Infrastructure` que fornece a connection string em design-time. Exemplo já adicionado ao projeto em `src/OficinaMecanica.Infrastructure/DesignTimeDbContextFactory.cs`.

## Verificar se as tabelas foram criadas

1. Confirme que o container está `RUNNING` e `healthy`.
2. Abra psql (veja seção acima) e rode `\dt` para listar tabelas.
3. Verifique a tabela de histórico de migrations:

```sql
SELECT * FROM "__EFMigrationsHistory";
```

## Boas práticas

- Não deixe senhas em texto em produção; use variáveis de ambiente ou secret manager.
- Prefira `Database.Migrate()` com controle (ou executar `dotnet ef database update`) em vez de `EnsureCreated()` em produção.
- Centralize versões de pacotes (Directory.Packages.props) para evitar conflitos.

---

Arquivo gerado automaticamente com os comandos e instruções essenciais para manipular o banco Postgres deste projeto.