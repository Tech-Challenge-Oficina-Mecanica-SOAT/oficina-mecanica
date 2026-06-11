# Oficina Mecânica — API

API REST desenvolvida em **ASP.NET Core (.NET 10)** como Tech Challenge da pós-graduação FIAP SOAT.  
Gerencia o ciclo completo de uma oficina mecânica: clientes, veículos, serviços, peças, ordens de serviço e autenticação por perfil.

---

## Índice

- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Como executar](#como-executar)
- [Documentação interativa (Scalar)](#documentação-interativa-scalar)
- [Collection das APIs](#collection-das-apis)
- [Autenticação](#autenticação)
- [Roteiros de teste](#roteiros-de-teste)
- [Cobertura de testes](#cobertura-de-testes)
- [Relatório de vulnerabilidades](#relatório-de-vulnerabilidades)
- [Scan de qualidade (SonarQube)](#scan-de-qualidade-sonarqube)
- [EF Core / Migrations](#ef-core--migrations)
- [Troubleshooting](#troubleshooting)

---

## Arquitetura

O projeto segue **Clean Architecture** com quatro camadas e regra de dependência estrita (dependências só apontam para dentro):

```
API  →  Application  →  Domain
 ↓           ↓
Infrastructure
```

### Camadas

| Camada | Projeto | Responsabilidade |
|---|---|---|
| **Domain** | `OficinaMecanica.Domain` | Entidades, Value Objects, Domain Events, interfaces de repositório |
| **Application** | `OficinaMecanica.Application` | Use Cases, DTOs, interfaces de infraestrutura, Result\<T\> pattern |
| **Infrastructure** | `OficinaMecanica.Infrastructure` | Repositórios EF Core, JWT, Argon2, logging, e-mail, dispatch de eventos |
| **API** | `OficinaMecanica.API` | Controllers, DI composition root, configuração |

### Estrutura de Application

```
Application/
├── Common/
│   └── Result.cs               # Result<T> — use cases não lançam exceções de negócio
├── Configuration/
│   └── IJwtSettings.cs         # Abstração de config (sem dependência de IConfiguration)
├── DTOs/
│   ├── Requests/               # Request Models (entrada dos use cases)
│   └── Responses/              # Response Models (saída dos use cases)
├── Interfaces/
│   ├── ITokenGenerator.cs      # Abstração de JWT (impl. em Infrastructure)
│   ├── IPasswordHasher.cs      # Abstração de Argon2 (impl. em Infrastructure)
│   └── IAppLogger.cs           # Abstração de logging (impl. em Infrastructure)
├── Mappers/
│   └── OrdemServicoMapper.cs   # Mapeamento entidade → DTO
└── UseCases/                   # 49 use cases, um por operação
    ├── Auth/
    ├── Cliente/
    ├── OrdemServico/
    ├── OrdemServicoStatus/
    ├── Peca/
    ├── Servico/
    └── Veiculo/
```

### Value Objects (Domain)

| VO | Regra |
|---|---|
| `Email` | Validado via `MailAddress.TryCreate`; armazenado em lowercase |
| `Documento` | CPF (11 dígitos) ou CNPJ numérico/alfanumérico Mercosul (14 chars) com dígitos verificadores |
| `Telefone` | 10–13 dígitos após limpeza |
| `Placa` | Formato antigo `AAA9999` ou Mercosul `AAA9A99` |

### Domain Events

Disparados pelas entidades e publicados automaticamente pelo `ApplicationDbContext.SaveChangesAsync`:
`OrcamentoEnviadoEvent` · `OrdemAprovadaEvent` · `OrdemRejeitadaEvent` · `OrdemConcluidaEvent` · `OrdemEntregueEvent`

> Decisões arquiteturais detalhadas em [`docs/adr/`](./docs/adr/).

---

## Pré-requisitos

| Ferramenta | Versão mínima |
|---|---|
| .NET SDK | 10.0 |
| Docker | 24+ |
| Docker Compose | 2.x |

---

## Como executar

### Com Docker (recomendado)

```bash
# 1. Subir PostgreSQL + API (build automático)
docker compose up -d --build

# 2. Verificar status
docker compose ps

# 3. Acompanhar logs
docker logs -f oficina_api
docker logs -f oficina_postgres
```

A API estará disponível em **`http://localhost:5000`**.

### Localmente (sem Docker)

```bash
# Requer PostgreSQL rodando localmente com as credenciais do appsettings.json
dotnet run --project src/OficinaMecanica.API
```

---

## Documentação interativa (Scalar)

Com a API no ar, acesse:

```
http://localhost:5000/scalar
```

Todas as rotas estão documentadas com descrição, parâmetros, exemplos de resposta e os perfis de acesso exigidos.

---

## Collection das APIs

A documentação completa da API está disponível via Scalar, que fornece uma interface interativa para explorar e testar todos os endpoints.

### Acesso interativo

- **Ambiente de desenvolvimento (Docker):** `http://localhost:5000/scalar/v1`
- **Ambiente local (dotnet run):** `http://localhost:5165/scalar/v1`

### Exportar collection

#### OpenAPI JSON

O documento OpenAPI (antigo Swagger) está disponível em:

- **Docker:** `http://localhost:5000/openapi/v1.json`
- **Local:** `http://localhost:5165/openapi/v1.json`

#### Postman

1. Acesse o link do OpenAPI JSON acima
2. Copie o conteúdo
3. No Postman, clique em **Import** → **Raw text** → cole o JSON → **Continue** → **Import**

Ou exporte via linha de comando:

```bash
# Instalar a ferramenta (apenas uma vez)
dotnet tool install -g Swashbuckle.AspNetCore.Cli

# Exportar o JSON
swagger tofile --output openapi.json src/OficinaMecanica.API/bin/Debug/net10.0/OficinaMecanica.API.dll v1
```

---

## Autenticação

A API usa **JWT Bearer Token**. O token é obtido via login e deve ser enviado no header de todas as rotas protegidas.

### Endpoint de login

```http
POST /Auth/login
Content-Type: application/json

{
  "email": "admin@oficina.com",
  "senha": "Senha@123"
}
```

Resposta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiracao": "2026-01-01T12:05:00Z"
}
```

Inclua o token em todas as requisições:
```
Authorization: Bearer {token}
```

> O token expira em **5 minutos**. Faça um novo login para renová-lo.

### Perfis de acesso

| Perfil | Valor | Acesso |
|---|---|---|
| `Admin` | `0` | Todas as rotas administrativas |
| `Mecanico` | `1` | Iniciar diagnóstico e notificar conclusão de OS |
| `Cliente` | `2` | Aprovar/rejeitar orçamento e consultar status da OS |

### Rotas públicas (sem token)

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/Auth/login` | Obter token JWT |
| `POST` | `/Auth/registrar` | Registrar usuário |
| `GET` | `/Publico/os/{id}/status` | Consultar status de uma OS sem autenticação |

### Rotas protegidas

Todas as demais rotas exigem `Authorization: Bearer {token}` com o perfil indicado:

| Recurso | Perfil exigido |
|---|---|
| Clientes, Veículos, Serviços, Peças | `Admin` |
| Ordens de serviço (CRUD e itens) | `Admin` |
| Iniciar diagnóstico / Notificar conclusão | `Admin` ou `Mecanico` |
| Aprovar / Rejeitar orçamento | `Admin` ou `Cliente` |
| Registrar entrega / Forçar status | `Admin` |
| Histórico de status | `Admin`, `Mecanico` ou `Cliente` |

---

## Roteiros de teste

Guias passo a passo por funcionalidade e um arquivo `.http` para uso no VS Code (extensão REST Client) estão em [`docs/testing/`](./docs/testing/README.md).

---

## Cobertura de testes

O projeto possui **476 testes** (392 unitários + 84 de integração) cobrindo use cases, entidades de domínio, Value Objects e todos os controllers.

### Gerar o relatório

```bash
# 1. Instalar o gerador de relatório (apenas uma vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# 2. Executar os testes coletando cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage-results --settings coverlet.runsettings

# 3. Gerar o relatório
reportgenerator -reports:"coverage-results/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"MHtml;TextSummary" -classfilters:"-Microsoft.AspNetCore.OpenApi.Generated;-System.Runtime.CompilerServices"
```

### Visualizar o relatório

Abra **`coverage-report/Summary.mht`** no **Edge ou Chrome** (Firefox não suporta `.mht`).  
O arquivo `coverage-report/Summary.txt` contém o resumo em texto puro.

### Resultado atual

| Camada | Cobertura de linhas |
|---|---|
| Application (services + DTOs) | **99.4%** |
| Domain (entidades) | **92.7%** |
| API (controllers) | **80.4%** |
| Infrastructure (repositories) | **80.0%** — exercitados via testes de integração com Testcontainers (PostgreSQL real) |
| **Total (linhas)** | **90.5%** |
| **Total (branches)** | **73.7%** |
| **Total (métodos)** | **93.9%** |

> `HistoricoStatusOSRepository` está em 0% pois o fluxo de histórico é coberto indiretamente pelos testes de integração de `OrdemServicoStatus`.

---

## Relatório de vulnerabilidades

A análise de segurança está documentada em **[`relatorio-vulnerabilidades.md`](./relatorio-vulnerabilidades.md)**, na raiz do repositório.

O relatório cobre:

- **10 achados** classificados por severidade (2 críticos, 3 altos, 4 médios, 1 baixo)
- Mapeamento contra **OWASP Top 10**
- Controles de segurança já implementados (hash timing-safe, JWT validado, EF Core parametrizado, usuário não-root no container)
- Instruções de correção para cada vulnerabilidade pendente

### Reproduzir a análise com SonarQube

O SonarQube está incluso no `docker-compose.yaml`. Veja a seção [Scan de qualidade (SonarQube)](#scan-de-qualidade-sonarqube) abaixo para executar um novo scan.

---

## Scan de qualidade (SonarQube)

### 1. Subir o SonarQube

```bash
docker compose up -d sonarqube
```

Aguarde ~1 minuto e acesse `http://localhost:9000`.  
Login padrão: **admin / admin** (será solicitada troca na primeira vez).

### 2. Criar projeto e token

1. Em `http://localhost:9000`, crie um projeto com a chave `mecanica-api`
2. Vá em **My Account → Security → Generate Token** e copie o token

### 3. Instalar o scanner (apenas uma vez)

```bash
dotnet tool install --global dotnet-sonarscanner
```

### 4. Executar o scan

> **Git Bash no Windows:** os argumentos `/k:` e `/d:` são convertidos incorretamente pelo MSYS. Use **PowerShell** ou **CMD**, ou prefixe o comando com `MSYS_NO_PATHCONV=1`.

**PowerShell / CMD:**
```powershell
dotnet sonarscanner begin /k:"mecanica-api" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="SEU_TOKEN" /d:sonar.exclusions="**/Migrations/**,**/obj/**"
dotnet build OficinaMecanica.slnx
dotnet sonarscanner end /d:sonar.token="SEU_TOKEN"
```

**Git Bash:**
```bash
MSYS_NO_PATHCONV=1 dotnet sonarscanner begin /k:"mecanica-api" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="SEU_TOKEN" /d:sonar.exclusions="**/Migrations/**,**/obj/**"
dotnet build OficinaMecanica.slnx
MSYS_NO_PATHCONV=1 dotnet sonarscanner end /d:sonar.token="SEU_TOKEN"
```

Resultados disponíveis em: `http://localhost:9000/dashboard?id=mecanica-api`

### 5. Scan com cobertura integrada

**PowerShell / CMD:**
```powershell
dotnet sonarscanner begin /k:"mecanica-api" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="SEU_TOKEN" /d:sonar.exclusions="**/Migrations/**,**/obj/**" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"
dotnet build OficinaMecanica.slnx
dotnet test --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
dotnet sonarscanner end /d:sonar.token="SEU_TOKEN"
```

---

## EF Core / Migrations

As migrations são aplicadas automaticamente no startup da API (`Database.Migrate()`).  
Para gerenciar manualmente:

```bash
# Instalar o EF CLI (apenas uma vez)
dotnet tool install --global dotnet-ef

# Criar nova migration
dotnet ef migrations add NomeDaMigration \
  --project src/OficinaMecanica.Infrastructure \
  --startup-project src/OficinaMecanica.API \
  --output-dir Migrations

# Aplicar migrations manualmente
dotnet ef database update \
  --project src/OficinaMecanica.Infrastructure \
  --startup-project src/OficinaMecanica.API
```

### Conectar ao banco via psql

```bash
docker exec -it oficina_postgres psql -U postgres -d OficinaDB

# Comandos úteis dentro do psql
\dt                                          -- listar tabelas
SELECT * FROM "__EFMigrationsHistory";       -- migrations aplicadas
```

---

## Troubleshooting

**API não conecta ao banco**  
Verifique se o container PostgreSQL está `healthy` antes de a API iniciar:
```bash
docker inspect --format '{{.State.Health.Status}}' oficina_postgres
```

**`dotnet ef` não encontrado**  
Confirme que o diretório de ferramentas do .NET está no PATH:
- Windows: `%USERPROFILE%\.dotnet\tools`
- Linux/macOS: `~/.dotnet/tools`

**`reportgenerator` não encontrado**  
Mesmo problema de PATH. Reinicie o terminal após instalar ou use o caminho completo.

**Porta já em uso**  
A API usa a porta `5000`. Verifique e encerre processos conflitantes:
```bash
# Windows
netstat -ano | findstr :5000

# Linux/macOS
lsof -i :5000
```
