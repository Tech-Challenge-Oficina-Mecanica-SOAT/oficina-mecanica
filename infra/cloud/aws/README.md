# Terraform — AWS EKS (Cloud)

Provisiona o cluster Kubernetes na AWS (EKS) com RDS PostgreSQL. Use para ambientes de produção ou staging em cloud.

## Recursos criados

| Recurso | Descrição |
|---------|-----------|
| **VPC** | VPC dedicada com subnets públicas e privadas em 2 AZs |
| **NAT Gateway** | Permite que nodes privados acessem a internet |
| **EKS Cluster** | Cluster Kubernetes gerenciado na AWS |
| **Node Group** | EC2 managed node group (padrão: `t3.small`, 1–3 nodes) |
| **RDS PostgreSQL 15** | Banco de dados gerenciado em subnet privada |
| **Security Group** | Permite acesso ao RDS apenas de dentro da VPC |
| **Manifestos K8s** | ConfigMap, Secret, Deployments, Services, HPA aplicados no cluster |

## Pré-requisitos

- [Terraform](https://developer.hashicorp.com/terraform/install) >= 1.6.0
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html) configurado (`aws configure`)
- [kubectl](https://kubernetes.io/docs/tasks/tools/) >= 1.28

## Configuração de credenciais

```bash
# Opção 1 — variável de ambiente
export TF_VAR_db_password="SuaSenhaSuperSecreta"

# Opção 2 — arquivo terraform.tfvars (não versionar no git!)
echo 'db_password = "SuaSenhaSuperSecreta"' > terraform.tfvars
```

Adicione `terraform.tfvars` ao `.gitignore`.

## Como aplicar

```bash
cd infra/cloud/aws

terraform init
terraform plan
terraform apply
```

## Configurar kubectl após o deploy

```bash
# O output 'configure_kubectl' mostra o comando exato, mas geralmente é:
aws eks update-kubeconfig --region us-east-1 --name oficina-mecanica
kubectl get nodes
```

## Atualizar a connection string no Secret K8s

Após o `terraform apply`, obtenha a connection string do RDS:

```bash
terraform output -raw rds_connection_string | base64
# Cole o valor em k8s/secret.yaml → ConnectionStrings__DefaultConnection
kubectl apply -f ../../../k8s/secret.yaml
```

## Variáveis principais

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `aws_region` | `us-east-1` | Região AWS |
| `cluster_name` | `oficina-mecanica` | Nome do cluster EKS |
| `node_instance_type` | `t3.small` | Tipo de instância dos nodes |
| `node_min_size` | `1` | Mínimo de nodes |
| `node_max_size` | `3` | Máximo de nodes |
| `db_instance_class` | `db.t3.micro` | Classe da instância RDS |
| `db_password` | — | **Obrigatório**, sem default |

## Destruir ambiente

```bash
terraform destroy
```
