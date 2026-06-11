# Infraestrutura — Oficina Mecânica

## Visão Geral

Este diretório contém toda a infraestrutura como código (IaC) do projeto, organizada em:

```
infra/
├── local/          # Terraform para cluster Kind (desenvolvimento/demo)
│   ├── main.tf
│   └── outputs.tf
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

### 1. Configurar o Secret antes de subir

Edite `k8s/secret.yaml` e substitua os valores placeholder pelas credenciais reais:

```yaml
stringData:
  Jwt__SecretKey: "sua-secret-key-com-minimo-32-caracteres"
  Seguranca__PasswordKey: "sua-password-key"
  ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=OficinaDB;Username=postgres;Password=sua-senha"
```

> ⚠️ Nunca commite o `secret.yaml` com valores reais. Adicione-o ao `.gitignore` ou use um gerenciador de secrets.

### 2. Subir o cluster e aplicar todos os manifestos

```bash
cd infra/local
terraform init
terraform apply
```

### 3. Verificar os pods

```bash
kubectl get pods
kubectl get svc
kubectl get hpa
```

### 4. Acessar a API localmente

```bash
kubectl port-forward svc/oficina-mecanica-api 5000:80
# Acesse: http://localhost:5000/scalar
```

### 5. Acessar o MailHog

```bash
kubectl port-forward svc/mailhog 8025:8025
# Acesse: http://localhost:8025
```

## Destruir o Ambiente

```bash
cd infra/local
terraform destroy
```

## Módulo de Banco (reutilizável)

O módulo `infra/modules/postgres` pode ser referenciado em outros ambientes (ex: staging) sem duplicar código:

```hcl
module "banco" {
  source           = "../modules/postgres"
  postgres_password = var.db_password
  storage_size      = "10Gi"
}
```

## Outputs

Após `terraform apply`:

| Output | Descrição |
|---|---|
| `cluster_name` | Nome do cluster Kind |
| `cluster_endpoint` | Endpoint do cluster |
| `kubeconfig_path` | Caminho do kubeconfig |
