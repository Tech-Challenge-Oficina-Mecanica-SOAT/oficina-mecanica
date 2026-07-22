# 00 — Setup de Infraestrutura

> Guia de configuração e execução do ambiente local via Kubernetes + Terraform.  
> Detalhamento técnico completo: [`docs/infra-detalhado.md`](../infra-detalhado.md)

---

## Pré-requisitos

Instale e verifique cada ferramenta antes de prosseguir:

| Ferramenta | Verificar com | Instalar |
|---|---|---|
| Docker Desktop (rodando) | `docker info` | [docker.com](https://www.docker.com/products/docker-desktop/) |
| Kind | `kind version` | `choco install kind` / `brew install kind` |
| Terraform | `terraform version` | `choco install terraform` / `brew install terraform` |
| kubectl | `kubectl version --client` | `choco install kubernetes-cli` / `brew install kubectl` |
| make | `make --version` | Git Bash (Windows) / nativo em macOS e Linux |

> **Windows:** execute os comandos abaixo no **Git Bash** (não no PowerShell nem no CMD).

---

## Passo a passo

### 1. Preparar o ambiente (uma vez)

```bash
make setup
```

O que acontece:
- Verifica que Docker, Kind, Terraform e kubectl estão instalados e acessíveis
- Confirma que o Docker Desktop está rodando
- Gera `k8s/secret.yaml` com credenciais de desenvolvimento prontas para uso

### 2. Subir o ambiente

```bash
make oficina-up
```

O que acontece (em ordem):
1. Build da imagem Docker da API
2. `terraform apply` — cria o cluster Kind, instala metrics-server e aplica todos os manifestos Kubernetes
3. Port-forwards em background para API e MailHog

Ao final, você verá:

```
===========================================
  API:     http://localhost:5000/scalar
  MailHog: http://localhost:8025
===========================================
```

### 3. Acessar

- **API (Scalar):** <http://localhost:5000/scalar>
- **MailHog:** <http://localhost:8025>

Para os roteiros de teste, siga os arquivos `01` a `09` nesta pasta.

---

## Encerrar

```bash
make oficina-down
```

Encerra os port-forwards e destrói o cluster Kind por completo (incluindo o banco de dados).

---

## Reiniciar do zero

Use quando houver erros de dados duplicados em uma segunda rodada de testes:

```bash
make oficina-reset
```

Equivale a `make oficina-down && make oficina-up`. O banco é recriado do zero.

---

## Troubleshooting

| Sintoma | Causa | Solução |
|---|---|---|
| `ERRO: Docker Desktop não está rodando` | Engine do Docker não iniciado | Abrir o Docker Desktop e aguardar `docker info` responder |
| `terraform apply` falha com `Plugin did not respond` / erro TLS | Antivírus com inspeção HTTPS ativa (AVG, Avast, Kaspersky, etc.) | Desativar Web Shield / inspeção HTTPS antes de rodar `make oficina-up` |
| `x509: certificate signed by unknown authority` em comandos kubectl | Comportamento cosmético do client Go com clusters Kind no Windows | `kubectl config set-cluster kind-oficina-mecanica --insecure-skip-tls-verify=true` |
| Porta 5000 já em uso | Outro processo usando a porta | `netstat -ano \| findstr :5000` (Windows) ou `lsof -i :5000` (Mac/Linux) para identificar e encerrar |
| `make: command not found` (Windows) | `make` não está no PATH | Instalar via `winget install ezwinports.make` no PowerShell como Admin, fechar e **reabrir o Git Bash** (não basta abrir nova aba) |
| `make: command not found` após instalação com winget | PATH atualizado apenas na sessão nova | Fechar completamente o Git Bash e reabrir; confirmar com `make --version` |
| `make` funciona no PowerShell mas não no Git Bash | Git Bash usa PATH próprio | Rodar `export PATH="/c/Users/$USER/AppData/Local/Microsoft/WinGet/Packages/ezwinports.make_Microsoft.Winget.Source_8wekyb3d8bbwe/bin":$PATH` ou instalar via `choco install make` |
