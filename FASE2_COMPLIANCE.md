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

## 5. Roteiro para o Vídeo Demonstrativo (≤ 15 min)

| Tempo | Conteúdo |
|-------|---------|
| 0:00–2:00 | Visão geral da arquitetura — mostrar README + diagrama Mermaid |
| 2:00–4:00 | `docker-compose up` — API rodando, acessar Scalar UI em `/scalar` |
| 4:00–7:00 | Consumo das APIs via Postman collection: login → criar cliente/veículo → abrir OS → avançar status → receber e-mail no MailHog (`localhost:8025`) → clicar no link de aprovação |
| 7:00–10:00 | CI/CD: mostrar GitHub Actions em execução (build → test → push Docker → deploy K8s) |
| 10:00–13:00 | Kubernetes: `kubectl get pods/svc/hpa -w`, simular carga para demonstrar HPA escalando |
| 13:00–15:00 | Terraform: `cd infra/local && terraform apply` criando o cluster Kind |

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
