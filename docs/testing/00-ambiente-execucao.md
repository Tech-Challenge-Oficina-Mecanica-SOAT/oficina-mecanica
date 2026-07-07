# Ambiente de execução — Docker, Kubernetes e Terraform

Guia operacional para instalar as ferramentas necessárias e subir o projeto antes de seguir os roteiros de teste deste diretório. Consolida o passo a passo validado de ponta a ponta em ambiente Windows (Docker Compose, Kubernetes manual e Terraform/IaC).

## Índice

- [Pré-requisitos](#pré-requisitos)
- [Instalando Kind e Terraform no Windows](#instalando-kind-e-terraform-no-windows)
- [Opção 1 — Docker Compose (recomendado para testar a API)](#opção-1--docker-compose-recomendado-para-testar-a-api)
- [Opção 2 — Kubernetes manual (sem Terraform)](#opção-2--kubernetes-manual-sem-terraform)
- [Opção 3 — Terraform (IaC)](#opção-3--terraform-iac)
- [Testando o autoscaling (HPA) de ponta a ponta](#testando-o-autoscaling-hpa-de-ponta-a-ponta)
- [Troubleshooting](#troubleshooting)

---

## Pré-requisitos

| Ferramenta | Versão mínima | Observação |
|---|---|---|
| .NET SDK | 10.0 | necessário só para rodar sem Docker |
| Docker Desktop | 24+ | **o engine precisa estar rodando** — sem isso `kind create cluster` falha com erro de pipe |
| Docker Compose | 2.x | |
| kubectl | 1.28+ | apenas para as opções 2 e 3 |
| Kind | 0.20+ | testado com `v0.31.0` — apenas para as opções 2 e 3 |
| Terraform | 1.6+ | testado com `v1.15.6` — apenas para a opção 3 |

---

## Instalando Kind e Terraform no Windows

Via **Chocolatey** (em PowerShell como Administrador):

```powershell
choco install kind terraform -y
```

Ou via **winget**:

```powershell
winget install Kubernetes.kind
winget install Hashicorp.Terraform
```

Confirme as instalações:

```powershell
kind version
terraform version
docker info   # confirma que o engine do Docker Desktop está de pé
```

---

## Opção 1 — Docker Compose (recomendado para testar a API)

A forma mais rápida de rodar o projeto para seguir os roteiros `01` a `09`:

```bash
docker compose up -d --build
docker compose ps
docker logs -f oficina_api
```

- API: `http://localhost:5000` (Scalar em `/scalar`)
- MailHog: `http://localhost:8025`

---

## Opção 2 — Kubernetes manual (sem Terraform)

Use para simular o ambiente de produção (K8s) localmente com um cluster Kind, ou para debugar passo a passo.

### 1. Criar o cluster Kind

```powershell
kind create cluster --name oficina-mecanica
```

> Se aparecer `x509: certificate signed by unknown authority` em comandos `kubectl` subsequentes, é um problema cosmético de verificação de certificado no Windows — a conectividade real funciona. Contorne com:
> ```powershell
> kubectl config set-cluster kind-oficina-mecanica --insecure-skip-tls-verify=true
> ```

### 2. Build da imagem da API e carga no cluster (sem registry)

```powershell
docker build -t oficina-mecanica-api:local .
kind load docker-image oficina-mecanica-api:local --name oficina-mecanica
```

### 3. Editar `k8s/secret.yaml` com credenciais reais

O secret tem 4 chaves:

```yaml
stringData:
  Jwt__SecretKey: "<sua-chave-com-32-chars>"
  Seguranca__PasswordKey: "<sua-chave>"
  PostgresPassword: "<sua-senha>"
  ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=OficinaDB;Username=postgres;Password=<sua-senha-igual-a-de-cima>"
```

> `PostgresPassword` e a senha embutida em `ConnectionStrings__DefaultConnection` **precisam ser idênticas** — uma alimenta o `POSTGRES_PASSWORD` do container do banco, a outra a string de conexão da API.

### 4. Aplicar os manifests, na ordem de dependência

```powershell
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/postgres-pvc.yaml
kubectl apply -f k8s/postgres-deployment.yaml
kubectl apply -f k8s/postgres-service.yaml
kubectl rollout status deployment/postgres --timeout=120s

kubectl apply -f k8s/mailhog-deployment.yaml
```

Para a API, como a imagem é local (não veio de um registry), o deployment precisa apontar para `oficina-mecanica-api:local` em vez do placeholder. Gere uma cópia local do manifest (sem alterar o arquivo versionado):

```bash
sed 's/IMAGE_TAG_PLACEHOLDER/oficina-mecanica-api:local/; \
     /image: oficina-mecanica-api:local/a\          imagePullPolicy: Never' \
  k8s/api-deployment.yaml > /tmp/api-deployment-local.yaml

kubectl apply -f /tmp/api-deployment-local.yaml
kubectl apply -f k8s/api-service.yaml
kubectl apply -f k8s/api-hpa.yaml
```

> Em ambiente real (CI/CD via GHCR, ou EKS), use `k8s/api-deployment.yaml` direto — o pipeline em `.github/workflows/ci.yml` já faz esse `sed` substituindo pela tag real da imagem publicada.

### 5. Instalar o metrics-server (obrigatório para o HPA funcionar)

Kind **não vem com metrics-server**. Sem ele, `kubectl get hpa` fica eternamente em `TARGETS: <unknown>/70%` e nunca escala.

```powershell
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml

# Em clusters Kind/local o kubelet usa certificado self-signed — necessário permitir TLS inseguro:
kubectl patch deployment metrics-server -n kube-system --type='json' `
  -p='[{"op":"add","path":"/spec/template/spec/containers/0/args/-","value":"--kubelet-insecure-tls"}]'

kubectl rollout status deployment/metrics-server -n kube-system --timeout=90s
```

Confirme que as métricas chegam (pode levar ~30-60s):

```powershell
kubectl top pods
kubectl get hpa
# TARGETS deve mostrar números reais, ex: cpu: 1%/70%, memory: 23%/80%
```

### 6. Acessar a API e o MailHog

```powershell
kubectl port-forward svc/oficina-mecanica-api 5000:80
# http://localhost:5000/scalar
# http://localhost:5000/health  -> "Healthy"
```

```powershell
kubectl port-forward svc/mailhog 8025:8025
# http://localhost:8025
```

> Se `/health` retornar `404` via port-forward mas a API estiver `1/1 Running`, é provável que outra coisa já esteja escutando na porta local 5000 (`netstat -ano | findstr :5000`). Use outra porta local: `kubectl port-forward svc/oficina-mecanica-api 5050:80`.

> ⚠️ Nunca commite o `k8s/secret.yaml` com valores reais. Adicione-o ao `.gitignore`.

---

## Opção 3 — Terraform (IaC)

O Terraform em `infra/local/` provisiona o cluster Kind, carrega a imagem da API, instala o metrics-server e aplica todos os manifests Kubernetes automaticamente.

> O módulo usa apenas os providers oficiais `hashicorp/null` e `hashicorp/local`, chamando os binários `kind`/`kubectl` via `local-exec` — evita depender de providers de comunidade não mantidos (`tehcyx/kind`, `gavinbunney/kubectl`).

### Pré-requisito crítico no Windows: antivírus com inspeção HTTPS

`terraform init`/`apply` pode falhar — **mesmo com providers oficiais da HashiCorp** — com:

```
Plugin did not respond... transport: authentication handshake failed:
tls: failed to verify certificate: x509: certificate signed by unknown authority
```

A causa costuma ser um antivírus com inspeção de tráfego HTTPS/Web Shield (AVG, Avast, Kaspersky, ESET, etc.) interceptando TLS até em conexões loopback (`127.0.0.1`), que é onde o Terraform troca dados com os plugins via mTLS.

**Correção:** desative a inspeção HTTPS/Web Shield do antivírus antes de rodar comandos Terraform (normalmente em Configurações → Proteção de Componentes → Proteção Web).

### Como usar

```bash
# 1. Build da imagem da API — feito FORA do Terraform.
#    (no Windows, chamar "docker build" como filho do processo terraform.exe
#    cancela a transferência do build context com "context canceled" — uma
#    interação específica do gerenciamento de processos do Terraform nesse SO.
#    Por isso o build é um passo manual antes do apply, e o Terraform só faz
#    o "kind load".)
docker build -t oficina-mecanica-api:local .

# 2. Editar k8s/secret.yaml com credenciais reais
#    (Jwt__SecretKey, Seguranca__PasswordKey, PostgresPassword e
#    ConnectionStrings__DefaultConnection — as duas últimas com a mesma senha)

# 3. Inicializar e aplicar
cd infra/local
terraform init
terraform plan
terraform apply -auto-approve

# 4. Ver outputs e verificar o cluster
terraform output
kubectl --context kind-oficina-mecanica get pods
kubectl --context kind-oficina-mecanica get hpa

# 5. Destruir o ambiente (remove o cluster Kind por completo)
terraform destroy -auto-approve
```

### Variáveis disponíveis (`infra/local/variables.tf`)

| Variável | Default | Descrição |
|---|---|---|
| `cluster_name` | `oficina-mecanica` | Nome do cluster Kind |
| `load_local_image` | `true` | Se `true`, espera uma imagem local (`docker build` prévio) e faz `kind load docker-image`. Se `false`, assume que `api_image` já está em um registry acessível pelo cluster |
| `api_image` | `oficina-mecanica-api:local` | Tag da imagem usada no Deployment |

Para usar uma imagem de um registry real (ex. produção/CI):

```bash
terraform apply -var="load_local_image=false" -var="api_image=ghcr.io/<usuario>/oficina-mecanica-api:sha-abc123"
```

---

## Testando o autoscaling (HPA) de ponta a ponta

Com o metrics-server instalado (Opção 2 ou 3), acompanhe o HPA em um terminal:

```bash
kubectl get hpa -w
```

Em outro terminal, gere carga. Um único `wget` em loop sequencial **não é suficiente** para passar de 70% de CPU (request da API é só `250m`); use 3–4 pods em paralelo:

```bash
for i in 1 2 3 4; do
  kubectl run load-test-$i --image=busybox --restart=Never -- \
    sh -c "while true; do wget -q -O- http://oficina-mecanica-api/health; done"
done
```

Resultado esperado: CPU sobe até ultrapassar 70% e o número de réplicas aumenta (min 2, max 10).

Para parar a carga e ver o scale-down:

```bash
kubectl delete pod load-test-1 load-test-2 load-test-3 load-test-4 --force --grace-period=0
```

O HPA tem uma janela de estabilização padrão de **~5 minutos** antes de reduzir réplicas (evita flapping) — não é instantâneo.

```bash
kubectl get all
kubectl describe hpa oficina-mecanica-api-hpa
```

---

## Troubleshooting

| Sintoma | Causa | Correção |
|---|---|---|
| `password authentication failed for user "postgres"` em loop na API | `k8s/postgres-deployment.yaml` lia `POSTGRES_PASSWORD` a partir da chave `ConnectionStrings__DefaultConnection` inteira, não só a senha | Chave dedicada `PostgresPassword` no secret, referenciada separadamente no deployment do Postgres |
| `kubectl get hpa` mostra `TARGETS: <unknown>/70%` para sempre | metrics-server não instalado (Kind não vem com ele) | Instalar `metrics-server` com `--kubelet-insecure-tls` |
| `x509: certificate signed by unknown authority` em qualquer comando `kubectl` | Bug de verificação de certificado do client Go no Windows com clusters Kind | `kubectl config set-cluster <nome> --insecure-skip-tls-verify=true` |
| `terraform apply` falha com `Plugin did not respond` / handshake TLS, mesmo com providers oficiais | Antivírus com inspeção HTTPS (AVG, Avast, Kaspersky, etc.) interceptando TLS em loopback | Desativar o Web Shield / inspeção HTTPS do antivírus antes de rodar comandos Terraform |
| `docker build` dentro do `local-exec` do Terraform falha com `error from sender: context canceled` | Interação específica do Windows entre o gerenciamento de processos do `terraform.exe` e o streaming do buildx | Build feito fora do Terraform; o Terraform só roda `kind load docker-image` de uma imagem já existente |
| `/health` retorna `404` via `kubectl port-forward`, mas pod está `1/1 Running` | Outro processo já escutando na porta local escolhida (ex. 5000) | Usar outra porta local no `port-forward` |
| `kind create cluster` falha com erro de pipe do Docker | Docker Desktop instalado mas o engine não está rodando | Abrir o Docker Desktop e aguardar `docker info` responder antes de criar o cluster |
| CPU do HPA não passa de ~58% mesmo sob carga | Um único pod `busybox` com `wget` sequencial gera pouca carga | Rodar 3+ pods de carga em paralelo |
| API reinicia uma vez no boot com `duplicate key value violates unique constraint "PK___EFMigrationsHistory"` | Race benigna: as réplicas do Deployment chamam `Database.Migrate()` simultaneamente no startup, uma delas perde a corrida | Não é um bug; o pod se recupera sozinho no restart seguinte |
| `dotnet ef` não encontrado | Diretório de ferramentas do .NET fora do PATH | Windows: `%USERPROFILE%\.dotnet\tools`; Linux/macOS: `~/.dotnet/tools` |
| Porta 5000 já em uso | Outro processo escutando na porta | Windows: `netstat -ano \| findstr :5000`; Linux/macOS: `lsof -i :5000` |
