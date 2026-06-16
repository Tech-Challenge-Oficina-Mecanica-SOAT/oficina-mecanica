# Oficina Mecânica — API

API REST desenvolvida em **ASP.NET Core (.NET 10)** como Tech Challenge da pós-graduação FIAP SOAT.  
Gerencia o ciclo completo de uma oficina mecânica: clientes, veículos, serviços, peças, ordens de serviço e autenticação por perfil.

---

## Índice

- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Como executar](#como-executar)
- [Kubernetes](#kubernetes)
- [Terraform (IaC)](#terraform-iac)
- [CI/CD](#cicd)
- [Documentação interativa (Scalar)](#documentação-interativa-scalar)
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

### Desenho da infraestrutura (Fase 2)

```mermaid
graph TD
    Internet([Internet]) --> Ingress[Ingress / Port-forward]
    Ingress --> API[API Deployment\n2-10 réplicas]
    API --> HPA[HPA\nCPU 70% / Mem 80%]
    API --> Postgres[(Postgres\nPVC 5Gi)]
    API --> MailHog[MailHog\nSMTP fake]
    API --> Webhook[Webhook Externo\nAprovação de Orçamento]

    subgraph Cluster Kind / K8s
        API
        HPA
        Postgres
        MailHog
    end

    subgraph Secrets K8s
        S1[Jwt__SecretKey]
        S2[ConnectionStrings]
        S3[PasswordKey]
    end

    API --> S1
    API --> S2
    API --> S3
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
| kubectl | 1.28+ (para K8s) |
| Kind | 0.20+ (para K8s local) |
| Terraform | 1.6+ (para IaC) |

---

## Como executar

### Com Docker (recomendado)

```bash
# 1. Subir PostgreSQL + API + MailHog (build automático)
docker compose up -d --build

# 2. Verificar status
docker compose ps

# 3. Acompanhar logs
docker logs -f oficina_api
docker logs -f oficina_postgres
```

A API estará disponível em **`http://localhost:5000`**.  
O MailHog (UI de e-mails) estará em **`http://localhost:8025`**.

### Localmente (sem Docker)

```bash
# Requer PostgreSQL rodando localmente com as credenciais do appsettings.json
dotnet run --project src/OficinaMecanica.API
```

---

## Kubernetes

Os manifestos estão em `k8s/` na raiz do repositório.

### Estrutura

```
k8s/
├── configmap.yaml           # Variáveis não-sensíveis (environment, JWT issuer/audience)
├── secret.yaml              # Variáveis sensíveis (JWT key, senha do banco)
├── postgres-pvc.yaml        # Volume persistente 5Gi
├── postgres-deployment.yaml
├── postgres-service.yaml
├── api-deployment.yaml      # 2 réplicas, readiness/liveness probe
├── api-service.yaml         # ClusterIP: 80 → 5000
├── api-hpa.yaml             # HPA: min=2 max=10 cpu=70% mem=80%
└── mailhog-deployment.yaml  # SMTP fake para dev
```

### Aplicar manualmente (sem Terraform)

```bash
# 1. Editar k8s/secret.yaml com as credenciais reais

# 2. Aplicar na ordem correta
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/postgres-pvc.yaml
kubectl apply -f k8s/postgres-deployment.yaml
kubectl apply -f k8s/postgres-service.yaml
kubectl apply -f k8s/mailhog-deployment.yaml
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/api-service.yaml
kubectl apply -f k8s/api-hpa.yaml

# 3. Verificar pods
kubectl get pods
kubectl get svc
kubectl get hpa

# 4. Acessar a API
kubectl port-forward svc/oficina-mecanica-api 5000:80
# http://localhost:5000/scalar

# 5. Acessar o MailHog
kubectl port-forward svc/mailhog 8025:8025
# http://localhost:8025
```

> ⚠️ Nunca commite o `secret.yaml` com valores reais. Adicione-o ao `.gitignore`.

---

## Terraform (IaC)

O Terraform provisiona automaticamente um cluster Kind local e aplica todos os manifestos Kubernetes.

### Estrutura

```
infra/
├── README.md
├── local/
│   ├── main.tf      # Cluster Kind + todos os kubectl_manifest
│   └── outputs.tf   # Outputs: cluster_name, endpoint, kubeconfig_path
└── modules/
    └── postgres/
        └── main.tf  # Módulo reutilizável do banco (fácil troca por RDS no futuro)
```

### Como usar

```bash
# Pré-requisitos: Kind e Terraform instalados

# 1. Editar k8s/secret.yaml com credenciais reais

# 2. Inicializar e aplicar
cd infra/local
terraform init
terraform apply

# 3. Ver outputs
terraform output

# 4. Destruir o ambiente
terraform destroy
```

### Recursos criados pelo Terraform

| Recurso | Descrição |
|---|---|
| `kind_cluster.oficina` | Cluster Kind com 1 control-plane + 1 worker |
| `kubectl_manifest.postgres_*` | PVC + Deployment + Service do Postgres |
| `kubectl_manifest.api_*` | Deployment + Service + HPA da API |
| `kubectl_manifest.mailhog` | Deployment + Service do MailHog |
| `kubectl_manifest.configmap` | ConfigMap com variáveis de ambiente |
| `kubectl_manifest.secret` | Secret com credenciais sensíveis |

---

## CI/CD

O pipeline está em `.github/workflows/ci.yml` e possui jobs separados por trigger:

### Em Pull Request (apenas testes)

```
build-and-test → dotnet restore + build + test
```

### Em push para main (build + deploy completo)

```
build-and-test → build-docker → deploy-banco → deploy-api
```

| Job | O que faz |
|---|---|
| `build-and-test` | Restore, build e testes automatizados |
| `build-docker` | Build e push da imagem para o GitHub Container Registry (GHCR) |
| `deploy-banco` | Sobe cluster Kind, aplica manifestos do Postgres |
| `deploy-api` | Aplica ConfigMap, Secret e manifestos da API; faz smoke test |

A imagem é publicada em:
```
ghcr.io/<seu-usuario>/oficina-mecanica-api:latest
ghcr.io/<seu-usuario>/oficina-mecanica-api:sha-<commit>
```

---

## Documentação interativa (Scalar)

Com a API no ar, acesse:

```
http://localhost:5000/scalar
```

Todas as rotas estão documentadas com descrição, parâmetros, exemplos de resposta e os perfis de acesso exigidos.

### Postman Collection

Importe a collection completa no Postman para testar todos os endpoints com variáveis de ambiente pré-configuradas:

- **Arquivo:** [`docs/oficina-mecanica.postman_collection.json`](docs/oficina-mecanica.postman_collection.json)

**Como importar:**
1. Abra o Postman → clique em **Import**
2. Selecione o arquivo acima
3. Configure a variável `baseUrl` se necessário (padrão: `http://localhost:5000`)
4. Faça **Auth → Login** primeiro — o token é salvo automaticamente nas variáveis da collection

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

**Pods não sobem no Kubernetes**  
```bash
kubectl describe pod <nome-do-pod>
kubectl logs <nome-do-pod>
```

**Terraform falha ao criar cluster**  
Certifique-se que o Docker está rodando antes de executar `terraform apply`.
