# Tech Challenge Fase 2 — Compliance Status

> Análise baseada no documento `tech_challenge_tarefas_destribuidas_v2.pdf`
> Última atualização: 2026-06-15

---

## Legenda

| Símbolo | Significado |
|---------|-------------|
| ✅ | Compliant — implementado e funcional |
| ⚠️ | Parcial — existe mas incompleto ou precisa de ajuste |
| ❌ | Faltando — não implementado |

---

## 1. Evolução da Aplicação

### 1.1 Refatoração do Código

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Clean Code (nomes claros, simplicidade, coesão) | ✅ | 49 use cases com nomes de domínio explícitos, sem abreviações. Value Objects com validação encapsulada. |
| Clean Architecture (separação de camadas e dependências) | ✅ | 4 camadas: Domain → Application → Infrastructure → API. Dependências sempre apontam para dentro. |
| Testes automatizados unitários | ✅ | 392 testes unitários cobrindo entidades, DTOs, value objects e JWT/Argon2. |
| Testes de integração | ✅ | 84 testes de integração via WebApplicationFactory + Testcontainers (PostgreSQL real). |
| Cobertura dos fluxos críticos | ✅ | 90.5% line coverage, 476 testes totais. Relatório em `coverlet.runsettings`. |

---

### 1.2 APIs

#### Abertura de Ordem de Serviço (OS)

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Receber dados do cliente, veículo, serviços e peças | ✅ | `POST /api/ordens-servico` — `AbrirOrdemServicoCommand` |
| Retornar identificação única da OS | ✅ | Retorna `OrdemServicoId` no response body |

#### Consulta de Status da OS

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Informar situação atual (todos os 7 status) | ✅ | `GET /Publico/os/{id}/status` — endpoint público sem auth |
| Status: Recebida, Diagnóstico, Aguardando Aprovação, Execução, Finalizada, Entregue | ✅ | Enum `StatusOS` com todos os valores + `Rejeitada` |

#### Aprovação de Orçamento

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Endpoint para receber notificações externas de aprovação ou recusa | ✅ | `WebhookController` — `GET /api/webhooks/ordens-servico/aprovar/{token}?aprovado=true/false` |
| Aprovar orçamento via API | ✅ | `PATCH /api/ordens-servico/{id}/aprovar` |
| Rejeitar orçamento via API | ✅ | `PATCH /api/ordens-servico/{id}/rejeitar` |

#### Listagem de Ordens de Serviço

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Ordenação por status: Em Execução > Aguardando Aprovação > Diagnóstico > Recebida | ✅ | `GET /api/ordens-servico/ordenadas` — `ListarOrdensOrdenadasQuery` |
| Mais antigas primeiro (dentro de cada status) | ✅ | Ordenação secundária por `DataAbertura ASC` |
| Exclusão lógica (não física) de OS Finalizadas e Entregues da listagem | ✅ | Filtro por status ativo no query — sem deleção física |

#### Atualização de Status da OS via E-mail

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| E-mail com link clicável enviado ao cliente | ✅ | `EmailNotificacaoService.EnviarOrcamentoAsync()` — envia HTML com botões "Aprovar / Recusar" |
| Link dispara mudança de status sem autenticação JWT | ✅ | `WebhookController` + `AprovarOrcamentoPorEmailUseCase` — token de uso único armazenado na OS |
| Notificações por e-mail nos demais status | ✅ | `EnviarAprovacaoAsync`, `EnviarRejeicaoAsync`, `EnviarConclusaoAsync`, `EnviarEntregaAsync` com HTML completo (implementado nesta sessão) |
| BaseUrl configurável via environment (não hardcoded) | ✅ | `EmailSettings.BaseUrl` + `configmap.yaml` `EmailSettings__BaseUrl` (corrigido nesta sessão) |

---

## 2. Infraestrutura

### 2.1 Conteinerização (Docker)

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Dockerfile atualizado | ✅ | Multi-stage build com .NET 10, non-root user, porta 5000 |
| docker-compose para desenvolvimento local | ✅ | `docker-compose.yaml` com Postgres, API, MailHog e SonarQube |

---

### 2.2 Orquestração com Kubernetes

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Deployments | ✅ | `k8s/api-deployment.yaml`, `k8s/postgres-deployment.yaml`, `k8s/mailhog-deployment.yaml` |
| Services | ✅ | `k8s/api-service.yaml`, `k8s/postgres-service.yaml` |
| ConfigMaps | ✅ | `k8s/configmap.yaml` — variáveis não-sensíveis |
| Secrets | ✅ | `k8s/secret.yaml` — dados sensíveis em base64 |
| Horizontal Pod Autoscaler (HPA) | ✅ | `k8s/api-hpa.yaml` — Min 2, Max 10 replicas; CPU 70%, Memória 80% |
| PersistentVolumeClaim para banco | ✅ | `k8s/postgres-pvc.yaml` — 5Gi, ReadWriteOnce |

---

### 2.3 Infraestrutura como Código (IaC — Terraform)

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Scripts Terraform para cluster Kubernetes local | ✅ | `infra/local/main.tf` — provider `tehcyx/kind`, Kind cluster com 1 control-plane + 1 worker |
| Banco de Dados provisionado via Terraform | ✅ | `infra/modules/postgres/main.tf` — módulo reutilizável (PVC + Deployment + Service) |
| Documentação dos recursos e como aplicar (local) | ✅ | `infra/README.md` com instruções de uso |
| Scripts Terraform para cluster Kubernetes em Cloud | ✅ | `infra/cloud/aws/` — EKS + VPC + RDS PostgreSQL (implementado nesta sessão) |
| Documentação dos recursos e como aplicar (cloud) | ✅ | `infra/cloud/aws/README.md` com pré-requisitos, variáveis e passo-a-passo |

---

### 2.4 CI/CD (GitHub Actions)

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Build da aplicação | ✅ | Job `build-and-test` — `dotnet build --configuration Release` |
| Execução dos testes automatizados | ✅ | Job `build-and-test` — `dotnet test` com xUnit |
| Build da imagem Docker | ✅ | Job `build-docker` — push para GHCR com tags `sha-{commit}` e `latest` |
| Deploy no cluster Kubernetes | ✅ | Job `deploy-api` — aplica manifestos com `kubectl apply` |
| Deploy do banco de dados | ✅ | Job `deploy-banco` — cria cluster Kind e aplica manifestos Postgres |
| Aplicação dos manifestos YAML no cluster | ✅ | `kubectl apply -f k8s/` com substituição de tag de imagem |

---

## 3. Entregáveis

### 3.1 Repositório Git

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Código-fonte atualizado e refatorado | ✅ | Clean Architecture, 4 projetos src |
| Dockerfile e docker-compose revisados | ✅ | Presentes na raiz do projeto |
| Manifestos Kubernetes em `/k8s` | ✅ | 9 arquivos YAML |
| Scripts Terraform em `/infra` | ✅ | `infra/local/` + `infra/modules/` + `infra/cloud/aws/` |
| Arquivos de configuração da pipeline CI/CD | ✅ | `.github/workflows/ci.yml` |

---

### 3.2 README.md

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Descrição da solução e objetivos desta fase | ✅ | README.md com descrição completa |
| Desenho da arquitetura (componentes da aplicação) | ✅ | Diagrama de Clean Architecture no README |
| Desenho da arquitetura (infraestrutura — diagrama Mermaid) | ✅ | Diagrama `graph TD` com todos os componentes K8s no README |
| Desenho da arquitetura (fluxo de deploy) | ✅ | CI/CD descrito com etapas no README |
| Instruções de execução local | ✅ | Docker Compose e .NET local documentados |
| Instruções de deploy em Kubernetes | ✅ | kubectl manual e via Terraform documentados |
| Instruções de provisionamento com Terraform | ✅ | `infra/README.md` e `infra/cloud/aws/README.md` |
| Link para collection das APIs | ✅ | `docs/oficina-mecanica.postman_collection.json` linkado no README (adicionado nesta sessão) |
| Link para vídeo demonstrativo (YouTube/Vimeo, até 15 min) | ❌ | **Pendente** — vídeo não gravado ainda |

---

## 4. Resumo Executivo

### O que está Compliant ✅ (30 de 31 itens)

Todos os requisitos técnicos estão implementados:

- Clean Code + Clean Architecture + 476 testes (90.5% cobertura)
- Todas as APIs de negócio com fluxo completo de e-mail
- Docker + docker-compose + K8s completo (Deployments, Services, ConfigMaps, Secrets, HPA, PVC)
- Terraform local (Kind) **e cloud (AWS EKS + RDS)**
- Pipeline CI/CD completa
- Postman collection exportada e linkada no README

### Único item pendente ❌ (1 item)

| # | Item | Ação necessária |
|---|------|----------------|
| 1 | **Vídeo demonstrativo** no YouTube/Vimeo linkado no README | Gravar e publicar (roteiro abaixo) |

---

## 5. Roteiro Detalhado para o Vídeo Demonstrativo (≤ 15 min)

> Use um gravador de tela (OBS Studio, Loom, etc.) com resolução mínima 1080p.
> Deixe todos os terminais, Postman e browser abertos antes de começar a gravar para não perder tempo.
> Fale em voz alta descrevendo o que está fazendo — o avaliador precisa entender sem ler o código.

---

### Cena 1 — Apresentação da Solução (0:00 – 2:00)

**O que mostrar:** Tela do README.md aberta no browser (GitHub) ou no VSCode Preview.

**O que dizer e fazer:**

1. Abra o repositório no GitHub e mostre a estrutura de pastas na raiz.
   > _"Este é o repositório do Tech Challenge Fase 2 da Oficina Mecânica. A solução foi construída em .NET 10 seguindo Clean Architecture com quatro camadas: Domain, Application, Infrastructure e API."_

2. Role até o diagrama Mermaid de infraestrutura no README.
   > _"A infraestrutura é composta por um cluster Kubernetes, onde a API escala automaticamente entre 2 e 10 réplicas via HPA, um PostgreSQL com volume persistente e um MailHog para captura de e-mails durante o desenvolvimento."_

3. Mostre rapidamente as pastas `k8s/`, `infra/` e `.github/workflows/` no explorador de arquivos.
   > _"Os manifestos Kubernetes estão em /k8s, o Terraform em /infra — com suporte a ambiente local via Kind e cloud via AWS EKS — e o pipeline CI/CD está configurado no GitHub Actions."_

---

### Cena 2 — Subindo o Ambiente Local (2:00 – 4:00)

**Pré-requisito:** Docker Desktop rodando.

**O que fazer:**

```bash
# Terminal 1 — na raiz do projeto
docker compose up -d --build
docker compose ps
```

> _"Com um único comando, temos a API, o PostgreSQL, o MailHog e o SonarQube no ar localmente. Na primeira execução usamos `--build` para garantir que a imagem esteja atualizada."_

Abra o browser em `http://localhost:5000/scalar`.

> _"A documentação interativa Scalar lista todos os endpoints com descrição, parâmetros e exemplos de resposta. Aqui vemos as rotas públicas, os endpoints protegidos por JWT e os webhooks."_

Mostre a resposta do endpoint de login: `POST http://localhost:5000/Auth/login`.

> _"A API está no ar — o endpoint de login responde com token JWT. O usuário admin padrão é criado automaticamente no primeiro boot pelo seed do Program.cs."_

---

### Cena 3 — Consumo das APIs (fluxo completo da OS) (4:00 – 7:00)

**Pré-requisito:** Postman aberto com a collection `docs/oficina-mecanica.postman_collection.json` importada. Variável `baseUrl` = `http://localhost:5000`.

**O que fazer e dizer, passo a passo:**

**3.1 — Login**
```
Auth → Login
Body: { "email": "admin@oficina.com", "senha": "Senha@123" }
```
> _"Faço login como Admin e o script da collection salva o token JWT automaticamente na variável `{{token}}`."_

Mostre que `pm.collectionVariables.set('token', ...)` capturou o valor.

**3.2 — Criar Cliente**
```
Clientes → Criar Cliente
Body: { "nome": "João Silva", "documento": "12345678901", "email": "joao@email.com", "telefone": "11999999999" }
```
> _"Crio um cliente. O ID é salvo automaticamente na variável `{{clienteId}}`."_

**3.3 — Criar Veículo**
```
Veículos → Criar Veículo
Body: { "clienteId": "{{clienteId}}", "placa": "ABC1234", "marca": "Toyota", "modelo": "Corolla", "ano": 2020 }
```
> _"Crio o veículo vinculado ao cliente. O valor object Placa valida o formato automaticamente."_

**3.4 — Abrir OS**
```
Ordens de Serviço → Abrir OS
Body: { "clienteId": "{{clienteId}}", "veiculoId": "{{veiculoId}}", "descricaoProblema": "Barulho ao frear" }
```
> _"Abro a Ordem de Serviço. O sistema retorna o ID único da OS com status `Recebida`."_

**3.5 — Listagem ordenada**
```
Ordens de Serviço → Listar OS Ordenadas por Status
```
> _"A listagem ordenada prioriza: Em Execução > Aguardando Aprovação > Diagnóstico > Recebida, e dentro de cada grupo as mais antigas aparecem primeiro. OS Finalizadas e Entregues são excluídas logicamente."_

**3.6 — Avançar status até AguardandoAprovacao**
```
Status da OS → 1 - Iniciar Diagnóstico     (PATCH /{osId}/iniciar-diagnostico)
Status da OS → 2 - Enviar para Aprovação   (PATCH /{osId}/status, novoStatus: 2)
```
> _"Avanço a OS pelo fluxo: Recebida → EmDiagnostico → AguardandoAprovacao. Cada transição é registrada no histórico."_

**3.7 — E-mail de orçamento no MailHog**

Abra o browser em `http://localhost:8025`.

> _"Quando a OS entra em AguardandoAprovacao, um domain event é disparado e a aplicação envia um e-mail ao cliente com os botões de Aprovar e Recusar. O MailHog captura esse e-mail localmente."_

Clique no e-mail recebido e mostre os botões HTML. Clique em **"APROVAR ORÇAMENTO"**.

> _"Ao clicar, o cliente acessa um endpoint público sem autenticação. O token de uso único é validado e o status da OS avança para EmExecucao automaticamente."_

**3.8 — Verificar status público (sem auth)**
```
Público → Consultar Status da OS
GET /Publico/os/{{osId}}/status
```
> _"Qualquer pessoa pode consultar o status da OS sem precisar de login — ideal para o cliente acompanhar pelo portal ou app."_

**3.9 — Histórico de status**
```
Status da OS → Histórico de Status
GET /api/ordens-servico/{{osId}}/historico
```
> _"O histórico completo registra cada transição com timestamp, usuário responsável e motivo."_

---

### Cena 4 — CI/CD no GitHub Actions (7:00 – 10:00)

**O que mostrar:** Aba Actions do repositório no GitHub.

**O que fazer e dizer:**

1. Abra a aba **Actions** e mostre o último workflow executado.
   > _"O pipeline é acionado em todo push para main ou pull request. Ele executa 4 jobs em sequência."_

2. Clique no job **build-and-test** e expanda os steps.
   > _"Primeiro: restore das dependências e build em modo Release. Em seguida, os 476 testes automatizados são executados — 392 unitários e 84 de integração com banco PostgreSQL real via Testcontainers."_

3. Clique no job **build-docker**.
   > _"Se os testes passam, a imagem Docker é construída via build multi-stage e publicada no GitHub Container Registry com a tag do commit SHA e a tag `latest`."_

4. Clique nos jobs **deploy-banco** e **deploy-api**.
   > _"Os dois últimos jobs criam o cluster Kind, aplicam os manifestos do banco e da API, substituem a tag da imagem pelo SHA do commit e fazem um smoke test no `/health` para confirmar que o deploy funcionou."_

---

### Cena 5 — Kubernetes e HPA (10:00 – 13:00)

**Pré-requisito:** Cluster Kind rodando (via `terraform apply` ou `kubectl apply` manual). Dois terminais abertos.

> O Kind não vem com `metrics-server` instalado por padrão. Sem ele, o HPA nunca calcula utilização (`TARGETS` fica em `<unknown>/70%`) e nunca escala. Instale antes de testar:
> ```bash
> kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
> # Em clusters Kind/local, o kubelet usa certificado self-signed; é necessário permitir TLS inseguro:
> kubectl patch deployment metrics-server -n kube-system --type='json' \
>   -p='[{"op":"add","path":"/spec/template/spec/containers/0/args/-","value":"--kubelet-insecure-tls"}]'
> kubectl rollout status deployment/metrics-server -n kube-system
> ```

> Os manifestos em `k8s/` não declaram `namespace`, então tudo é aplicado no namespace `default`. Exponha a API com port-forward antes de iniciar os testes (nome do Service real é `oficina-mecanica-api`, não `oficina-api-service`):
> ```bash
> kubectl port-forward svc/oficina-mecanica-api 5000:80
> # http://localhost:5000/scalar
> ```

**Terminal 1 — monitorar HPA em tempo real:**
```bash
kubectl get hpa -w
```

**Terminal 2 — simular carga:**
```bash
kubectl run -it --rm load-test --image=busybox --restart=Never -- \
  sh -c "while true; do wget -q -O- http://oficina-mecanica-api/health; done"
```

> _"O HPA `oficina-mecanica-api-hpa` está configurado para escalar entre 2 e 10 réplicas quando o consumo de CPU ultrapassar 70% ou a memória 80%. Vou simular carga para demonstrar o escalonamento automático."_

> O HPA só reavalia métricas a cada ~15s (intervalo padrão do `--horizontal-pod-autoscaler-sync-period`) e o `metrics-server` coleta a cada ~60s. Espere de 1 a 3 minutos para ver `TARGETS` subir e o `REPLICAS` escalar no Terminal 1 — não é instantâneo.

```bash
kubectl get pods -w
```
> _"Novos pods são criados automaticamente para absorver a carga. Quando a carga cessa, o HPA reduz as réplicas de volta ao mínimo de 2."_

Mostre também:
```bash
kubectl get all
kubectl describe hpa oficina-mecanica-api-hpa
```

---

### Cena 6 — Terraform (13:00 – 15:00)

**O que mostrar:** Terminal na pasta `infra/local/`.

> ⚠️ **Pré-requisito importante (Windows):** se você tiver antivírus com inspeção de tráfego HTTPS (AVG, Avast, Kaspersky, etc.), desative essa proteção antes de rodar `terraform init`/`apply`. Esses antivírus interceptam TLS até em conexões loopback (`127.0.0.1`), o que quebra o handshake mTLS que o Terraform usa para se comunicar com os plugins de provider — o erro aparece como `Plugin did not respond` / `x509: certificate signed by unknown authority`, mesmo com providers oficiais da HashiCorp. Achado e corrigido nesta sessão: ver detalhe em [`README_FASE2.md`](./README_FASE2.md#terraform-iac--corrigido-e-validado).

> ℹ️ Os providers de comunidade usados antes aqui (`tehcyx/kind`, `gavinbunney/kubectl`) estavam **abandonados desde 2021** e foram **removidos**. O `infra/local/main.tf` atual usa só os providers oficiais `hashicorp/null` e `hashicorp/local`, chamando os binários `kind`/`kubectl` via `local-exec` — mesmo resultado, sem dependência de plugin não mantido.

**1. Build da imagem da API** (passo manual, fora do Terraform — ver por quê em [`README_FASE2.md`](./README_FASE2.md#terraform-iac--corrigido-e-validado)):
```bash
docker build -t oficina-mecanica-api:local .
```

**2. Editar `k8s/secret.yaml`** com credenciais reais (`Jwt__SecretKey`, `Seguranca__PasswordKey`, `PostgresPassword` e `ConnectionStrings__DefaultConnection` — as duas últimas com a **mesma senha**).

**3. Inicializar e aplicar:**
```bash
cd infra/local
terraform init
terraform plan
```

> _"O Terraform provisiona o cluster Kind localmente com um control-plane e um worker node, carrega a imagem da API já buildada para dentro do cluster, instala o metrics-server e aplica todos os manifestos Kubernetes em ordem de dependência — Postgres, MailHog, API, Service e HPA."_

```bash
terraform apply -auto-approve
```

Mostre os recursos sendo criados no output (13 recursos: cluster, carga de imagem, secret, configmap, PVC, Postgres, MailHog, metrics-server, API, Service, HPA).

**4. Verificar o resultado:**
```bash
kubectl --context kind-oficina-mecanica get pods
kubectl --context kind-oficina-mecanica get hpa
```

**5. Destruir o ambiente ao final:**
```bash
terraform destroy -auto-approve
```
Isso apaga o cluster Kind por completo (não precisa deletar recursos um a um).

> _"Para ambientes cloud, temos também o módulo `infra/cloud/aws/` que provisiona um cluster EKS na AWS com VPC dedicada, subnets públicas e privadas, e banco PostgreSQL gerenciado no RDS — tudo com um único `terraform apply`."_

Abra o arquivo `infra/cloud/aws/main.tf` no editor e mostre brevemente os blocos de VPC, EKS e RDS.

> _"Todos os recursos são documentados em `infra/cloud/aws/README.md` com instruções de pré-requisitos, variáveis e como obter a connection string do RDS após o provisionamento."_

---

### Encerramento (14:30 – 15:00)

> _"Para resumir: evoluímos a aplicação da Fase 1 com Clean Architecture, 476 testes automatizados com 90,5% de cobertura, containerização Docker, orquestração Kubernetes com HPA, infraestrutura como código com Terraform local e cloud, pipeline CI/CD completa no GitHub Actions e notificações por e-mail com link de aprovação de orçamento. Obrigado."_

---

## 5b. Tabela-Resumo do Roteiro (referência rápida)

| Tempo | Cena | Ferramenta |
|-------|------|-----------|
| 0:00–2:00 | Apresentação da solução e arquitetura | GitHub / VSCode |
| 2:00–4:00 | Ambiente local — docker-compose up + Scalar UI | Terminal / Browser |
| 4:00–7:00 | Fluxo completo da OS + aprovação via e-mail | Postman + MailHog |
| 7:00–10:00 | Pipeline CI/CD rodando | GitHub Actions |
| 10:00–13:00 | Kubernetes + HPA escalando | kubectl / Terminal |
| 13:00–15:00 | Terraform local + visão geral do módulo AWS | Terminal / Editor |

**Após gravar**, adicionar no README:
```markdown
## Vídeo Demonstrativo
[Assista no YouTube](https://youtu.be/LINK_AQUI)
```

---

## 6. Checklist Final para Submissão

- [x] Item 1: Fluxo de e-mail completo (link clicável + notificações em todos os status)
- [x] Item 3: Postman collection exportada em `docs/` e linkada no README
- [x] Item 4: Terraform para AWS EKS em `infra/cloud/aws/`
- [ ] **PENDENTE:** Gravar vídeo e adicionar link no README
- [ ] Verificar que o repositório está compartilhado com `soat-architecture` no GitHub
- [ ] Gerar PDF com link do repositório + desenho de arquitetura + link do vídeo para entrega no portal do aluno

---

## 7. Alterações Realizadas Nesta Sessão

| Arquivo | O que mudou |
|---------|-------------|
| `src/OficinaMecanica.API/Program.cs` | Adicionado seed do usuário admin padrão (`admin@oficina.com` / `Senha@123`) no primeiro boot |
| `docker-compose.yaml` | Adicionadas variáveis de ambiente `Jwt__*`, `Seguranca__PasswordKey` e `EmailSettings__BaseUrl` para o container da API |
| `src/OficinaMecanica.Infrastructure/Configuration/EmailSettings.cs` | Adicionada propriedade `BaseUrl` |
| `src/OficinaMecanica.Infrastructure/Notifications/EmailNotificacaoService.cs` | Substituído `baseUrl` hardcoded por `_emailSettings.BaseUrl`; implementado envio real de HTML para aprovação, rejeição, conclusão e entrega |
| `src/OficinaMecanica.API/appsettings.json` | Adicionada seção `EmailSettings` com `BaseUrl` |
| `k8s/configmap.yaml` | Adicionada chave `EmailSettings__BaseUrl` |
| `docs/oficina-mecanica.postman_collection.json` | **Novo** — collection completa com todas as rotas, variáveis e scripts de auto-salvar IDs |
| `infra/cloud/aws/main.tf` | **Novo** — VPC + EKS + RDS + manifestos K8s via Terraform |
| `infra/cloud/aws/variables.tf` | **Novo** — variáveis configuráveis do módulo AWS |
| `infra/cloud/aws/outputs.tf` | **Novo** — outputs: cluster endpoint, kubeconfig cmd, RDS connection string |
| `infra/cloud/aws/README.md` | **Novo** — documentação do módulo cloud |
| `README.md` | Adicionada seção de Postman collection com instruções de importação |
| `FASE2_COMPLIANCE.md` | Atualizado com status real e histórico desta sessão |
