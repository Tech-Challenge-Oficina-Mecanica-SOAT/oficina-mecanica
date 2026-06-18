# Infraestrutura — Oficina Mecânica

## Visão Geral

Este diretório contém toda a infraestrutura como código (IaC) do projeto, organizada em:

```
infra/
├── local/          # Terraform para cluster Kind (desenvolvimento/demo)
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   └── kind-config.yaml
└── modules/
    └── postgres/   # Módulo reutilizável do banco de dados
        └── main.tf

k8s/
├── configmap.yaml          # Variáveis de ambiente não-sensíveis
├── secret.yaml             # Variáveis sensíveis (JWT, senha do banco)
├── postgres-pvc.yaml       # Volume persistente do Postgres
├── postgres-deployment.yaml
├── postgres-service.yaml
├── api-deployment.yaml
├── api-service.yaml        # ClusterIP: 80 → 5000
├── api-hpa.yaml            # HPA: min=2 max=10 cpu=70%
└── mailhog-deployment.yaml # SMTP fake para desenvolvimento
```

> `infra/local` usa só os providers oficiais `hashicorp/null` e `hashicorp/local`, que chamam os binários `kind`/`kubectl` via `local-exec`. Não depende de providers de comunidade para Kind/Kubernetes.

## Recursos Criados

| Recurso | Descrição |
|---|---|
| Kind Cluster | Cluster Kubernetes local com 1 control-plane + 1 worker |
| Postgres Deployment | Banco PostgreSQL 15 com volume persistente (5Gi) |
| Postgres Service | ClusterIP expondo porta 5432 internamente |
| API Deployment | 2 réplicas da API com readiness/liveness probe |
| API Service | ClusterIP expondo porta 80 → 5000 |
| HPA | Escala de 2 a 10 réplicas com base em CPU (70%) e memória (80%) |
| MailHog | SMTP fake acessível em porta 1025 (SMTP) e 8025 (UI) |

## Pré-requisitos

```bash
# Instalar Kind
brew install kind       # macOS
choco install kind      # Windows

# Instalar Terraform
brew install terraform  # macOS
choco install terraform # Windows

# Instalar kubectl
brew install kubectl
```

## Como Aplicar

### 1. Buildar a imagem da API (fora do Terraform)

```bash
docker build -t oficina-mecanica-api:local .
```

> No Windows, chamar `docker build` como filho do processo `terraform.exe` cancela a transferência do build context (`error from sender: context canceled`) — uma interação específica do gerenciamento de processos do Terraform nesse SO. Por isso o build é manual e o Terraform só carrega a imagem já existente no cluster (`kind load docker-image`).

### 2. Configurar o Secret antes de subir

Edite `k8s/secret.yaml` e substitua os valores placeholder pelas credenciais reais:

```yaml
stringData:
  Jwt__SecretKey: "sua-secret-key-com-minimo-32-caracteres"
  Seguranca__PasswordKey: "sua-password-key"
  PostgresPassword: "sua-senha"
  ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=OficinaDB;Username=postgres;Password=sua-senha"
```

> `PostgresPassword` e a senha embutida em `ConnectionStrings__DefaultConnection` precisam ser **idênticas** — uma alimenta o `POSTGRES_PASSWORD` do container do banco, a outra a string de conexão da API.

> ⚠️ Nunca commite o `secret.yaml` com valores reais. Adicione-o ao `.gitignore` ou use um gerenciador de secrets.

> ⚠️ No Windows, se você tiver antivírus com inspeção de tráfego HTTPS (AVG, Avast, Kaspersky, etc.), desative-a antes do `terraform init`/`apply` — esses antivírus interceptam TLS até em loopback (`127.0.0.1`), quebrando o handshake mTLS que o Terraform usa para falar com os plugins de provider (erro `Plugin did not respond` / `x509: certificate signed by unknown authority`, mesmo com providers oficiais da HashiCorp).

### 3. Subir o cluster e aplicar todos os manifestos

```bash
cd infra/local
terraform init
terraform apply -auto-approve
```

### 4. Verificar os pods

```bash
kubectl --context kind-oficina-mecanica get pods
kubectl --context kind-oficina-mecanica get svc
kubectl --context kind-oficina-mecanica get hpa
```

### 5. Acessar a API localmente

```bash
kubectl --context kind-oficina-mecanica port-forward svc/oficina-mecanica-api 5000:80
# Acesse: http://localhost:5000/scalar
```

### 6. Acessar o MailHog

```bash
kubectl --context kind-oficina-mecanica port-forward svc/mailhog 8025:8025
# Acesse: http://localhost:8025
```

## Destruir o Ambiente

```bash
cd infra/local
terraform destroy -auto-approve
```

Isso apaga o cluster Kind por completo — não é necessário remover os recursos um a um.

## Módulo de Banco (reutilizável)

O módulo `infra/modules/postgres` pode ser referenciado em outros ambientes (ex: staging) sem duplicar código:

```hcl
module "banco" {
  source           = "../modules/postgres"
  postgres_password = var.db_password
  storage_size      = "10Gi"
}
```

## Variáveis (`infra/local/variables.tf`)

| Variável | Default | Descrição |
|---|---|---|
| `cluster_name` | `oficina-mecanica` | Nome do cluster Kind |
| `load_local_image` | `true` | Se `true`, espera uma imagem local (`docker build` prévio) e faz `kind load docker-image`. Se `false`, assume que `api_image` já está publicada em um registry acessível pelo cluster |
| `api_image` | `oficina-mecanica-api:local` | Tag da imagem usada no Deployment da API |

Exemplo usando uma imagem já publicada (CI/produção):
```bash
terraform apply -var="load_local_image=false" -var="api_image=ghcr.io/<usuario>/oficina-mecanica-api:sha-abc123"
```

## Outputs

Após `terraform apply`:

| Output | Descrição |
|---|---|
| `cluster_name` | Nome do cluster Kind |
| `kubeconfig_context` | Contexto kubectl do cluster (`kind-<cluster_name>`) |
| `port_forward_api` | Comando pronto para expor a API localmente |
| `port_forward_mailhog` | Comando pronto para expor o MailHog localmente |
