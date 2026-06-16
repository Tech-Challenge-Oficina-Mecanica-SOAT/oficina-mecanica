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
| Endpoint para receber notificações externas de aprovação ou recusa | ✅ | `WebhookController` — integração com ferramenta externa |
| Aprovar orçamento | ✅ | `PATCH /api/ordens-servico/{id}/aprovar` |
| Rejeitar orçamento | ✅ | `PATCH /api/ordens-servico/{id}/rejeitar` |

#### Listagem de Ordens de Serviço

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Ordenação por status: Em Execução > Aguardando Aprovação > Diagnóstico > Recebida | ✅ | `GET /api/ordens-servico/ordenadas` — `ListarOrdensOrdenadasQuery` |
| Mais antigas primeiro (dentro de cada status) | ✅ | Ordenação secundária por `DataAbertura ASC` |
| Exclusão lógica (não física) de OS Finalizadas e Entregues da listagem | ✅ | Filtro por status ativo no query — sem deleção física |

#### Atualização de Status da OS via ferramenta externa (e-mail)

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Atualização via e-mail ou ferramenta similar | ⚠️ | `MailHog` presente no docker-compose e K8s para receber e-mail. Existe envio de notificação por e-mail via domain events (`OrcamentoEnviadoEvent`, etc.), mas **não há mecanismo de atualização de status acionado pelo recebimento de e-mail** (ex.: polling de caixa de entrada, webhook de e-mail, ou link de aprovação no e-mail). O `WebhookController` cobre o fluxo externo, mas não é especificamente via e-mail. |

> **Como implementar:** Adicionar um link de aprovação/rejeição no corpo do e-mail enviado ao cliente (gerado com token assinado), apontando para um endpoint público. Ao clicar, o cliente dispara a atualização de status sem precisar de autenticação JWT. Fluxo: `OrcamentoEnviadoEvent` → e-mail com link → `GET /Publico/os/{id}/aprovar?token=xxx` → valida token → atualiza status.

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
| Services | ✅ | `k8s/api-service.yaml`, `k8s/postgres-service.yaml` (MailHog Service embutido no deployment) |
| ConfigMaps | ✅ | `k8s/configmap.yaml` — variáveis não-sensíveis (ASPNETCORE_ENVIRONMENT, JWT config, Email) |
| Secrets | ✅ | `k8s/secret.yaml` — dados sensíveis em base64 (Jwt__SecretKey, ConnectionStrings, PasswordKey) |
| Horizontal Pod Autoscaler (HPA) | ✅ | `k8s/api-hpa.yaml` — Min 2, Max 10 replicas; CPU 70%, Memória 80% |
| PersistentVolumeClaim para banco | ✅ | `k8s/postgres-pvc.yaml` — 5Gi, ReadWriteOnce |

---

### 2.3 Infraestrutura como Código (IaC — Terraform)

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Scripts Terraform para provisionamento do cluster Kubernetes | ✅ | `infra/local/main.tf` — provider `tehcyx/kind`, cria cluster com 1 control-plane + 1 worker |
| Aplica manifestos K8s via Terraform | ✅ | Provider `gavinbunney/kubectl` aplica todos os YAMLs do `/k8s` |
| Banco de Dados provisionado via Terraform | ✅ | `infra/modules/postgres/main.tf` — módulo reutilizável (PVC + Deployment + Service) |
| Documentação dos recursos e como aplicar | ✅ | `infra/README.md` com instruções de uso |
| Suporte a Cloud (além de local) | ❌ | Apenas Kind local implementado. Sem módulo para EKS, GKE ou AKS. |

> **Como implementar (Cloud):** Criar `infra/cloud/` com provider AWS (`hashicorp/aws`) ou GCP (`hashicorp/google`). Para AWS: módulo EKS com node groups, IAM roles, VPC, RDS PostgreSQL (ou usar o módulo `postgres` adaptado). Usar `terraform workspace` para separar ambientes (dev/prod). Documentar variáveis sensíveis como `AWS_ACCESS_KEY_ID` como GitHub Secrets.

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
| Scripts Terraform em `/infra` | ✅ | `infra/local/` e `infra/modules/` |
| Arquivos de configuração da pipeline CI/CD | ✅ | `.github/workflows/ci.yml` |

---

### 3.2 README.md

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| Descrição da solução e objetivos desta fase | ✅ | README.md tem 532 linhas com descrição completa |
| Desenho da arquitetura (componentes da aplicação) | ✅ | Diagrama de Clean Architecture no README |
| Desenho da arquitetura (infraestrutura provisionada) | ⚠️ | Descrito em texto, mas sem diagrama visual (imagem/ASCII art) explícito dos recursos K8s/Terraform |
| Desenho da arquitetura (fluxo de deploy) | ⚠️ | CI/CD descrito em prosa, mas sem diagrama visual do pipeline |
| Instruções de execução local | ✅ | Docker Compose e .NET local documentados |
| Instruções de deploy em Kubernetes | ✅ | kubectl manual e via Terraform documentados |
| Instruções de provisionamento com Terraform | ✅ | `infra/README.md` com passo-a-passo |
| Link para collection das APIs (Postman/Swagger) | ⚠️ | Existe Scalar UI em `/scalar`, mas **falta link externo** para Postman collection ou Swagger JSON exportado |
| Link para vídeo demonstrativo (YouTube/Vimeo, até 15 min) | ❌ | **Não encontrado no README** |

> **Como implementar (Vídeo):** Gravar demonstração cobrindo: (1) execução do CI/CD pipeline no GitHub Actions, (2) aplicação rodando via `docker-compose up`, (3) consumo das APIs via Scalar ou Postman, (4) simulação de carga com HPA em ação (`kubectl get hpa -w`). Publicar no YouTube (não listado) e adicionar link no README.

> **Como implementar (API Collection):** Exportar a coleção do Postman ou o Swagger JSON (`/scalar` já serve o OpenAPI spec) e hospedar/linkar no README. Alternativa: adicionar link para `https://{repo}/blob/main/docs/oficina-mecanica.postman_collection.json`.

---

## 4. Resumo Executivo

### O que está Compliant ✅ (27 de 31 itens)

- Clean Code + Clean Architecture com 4 camadas bem definidas
- 476 testes (392 unit + 84 integration) com 90.5% de cobertura
- Todas as APIs de negócio implementadas (abertura OS, consulta status, aprovação, listagem ordenada)
- Docker (Dockerfile multi-stage + docker-compose completo)
- Kubernetes com todos os recursos: Deployments, Services, ConfigMaps, Secrets, HPA, PVC
- Terraform para provisionamento local (Kind cluster + Postgres + aplicação)
- Pipeline CI/CD completa (build → test → docker build → deploy K8s)
- README extenso com instruções de execução

### O que está Faltando / Parcial ❌⚠️ (4 itens)

| # | Item | Prioridade | Esforço Estimado |
|---|------|-----------|-----------------|
| 1 | **Atualização de status via e-mail** (link clicável no e-mail dispara mudança de status) | Alta | Médio — 1 endpoint público + geração de token no e-mail |
| 2 | **Vídeo demonstrativo** no README (YouTube/Vimeo, até 15 min) | Alta | Médio — gravação e edição |
| 3 | **Link para API collection** exportada (Postman/Swagger JSON) | Média | Baixo — exportar do Scalar e linkar |
| 4 | **Terraform para Cloud** (EKS/GKE/AKS) | Baixa | Alto — novo módulo Terraform com provider cloud |

---

## 5. Plano de Implementação dos Itens Faltantes

### Item 1 — Atualização de Status via E-mail ⚠️

**Objetivo:** Quando a OS entra em `AguardandoAprovacao`, o cliente recebe um e-mail com link de aprovação/rejeição. Ao clicar, o status é atualizado sem autenticação JWT.

**Passos:**

1. Criar `EmailApprovalTokenService` na camada Infrastructure:
   - Gerar token JWT de curta duração (ex: 48h) com `claims: { osId, acao: "aprovar"|"rejeitar" }`
   - Assinar com a `Jwt__SecretKey` já existente

2. Modificar o handler de `OrcamentoEnviadoEvent` para incluir os links no corpo do e-mail:
   ```
   https://{base-url}/Publico/os/{id}/resposta-orcamento?token={jwt}
   ```

3. Criar endpoint público em `PublicoController`:
   ```csharp
   GET /Publico/os/{id}/resposta-orcamento?token={jwt}&acao={aprovar|rejeitar}
   ```
   - Valida token, extrai `osId` e `acao`
   - Chama `AprovarOrcamentoCommand` ou `RejeitarOrcamentoCommand`
   - Retorna página HTML simples de confirmação (ou redirect)

4. Adicionar testes de integração para o novo endpoint

5. Atualizar `k8s/configmap.yaml` com `Email__BaseUrl` e `k8s/secret.yaml` com token secret se necessário

**Arquivos a modificar:**
- `src/OficinaMecanica.Infrastructure/Notifications/EmailService.cs` — incluir links no template
- `src/OficinaMecanica.API/Controllers/PublicoController.cs` — novo endpoint
- `src/OficinaMecanica.Application/UseCases/OrdemServico/` — novo use case ou reusar existente
- `tests/OficinaMecanica.Tests.Integration/Controllers/PublicoControllerTests.cs` — novos testes

---

### Item 2 — Vídeo Demonstrativo ❌

**Roteiro sugerido (≤ 15 min):**

| Tempo | Conteúdo |
|-------|---------|
| 0:00–2:00 | Visão geral da arquitetura (mostrar README + diagrama) |
| 2:00–4:00 | `docker-compose up` — API rodando localmente, Scalar UI |
| 4:00–7:00 | Consumo das APIs: login → criar OS → consultar status → aprovar → receber e-mail |
| 7:00–10:00 | CI/CD: mostrar GitHub Actions em execução (build → test → deploy) |
| 10:00–13:00 | Kubernetes: `kubectl get pods/svc/hpa`, demonstrar HPA com carga |
| 13:00–15:00 | Terraform: `terraform apply` criando o cluster Kind |

**Após gravar:** Adicionar no README:
```markdown
## Vídeo Demonstrativo
[Assista no YouTube](https://youtu.be/XXXX)
```

---

### Item 3 — API Collection Exportada ⚠️

**Passos:**

1. Com a API rodando (`docker-compose up`), acessar `http://localhost:5000/openapi/v1.json`
2. Importar no Postman via "Import from URL"
3. Adicionar variáveis de ambiente no Postman (`{{baseUrl}}`, `{{token}}`)
4. Exportar como `docs/oficina-mecanica.postman_collection.json`
5. Commitar o arquivo e adicionar no README:
   ```markdown
   ## API Collection
   - [Postman Collection](docs/oficina-mecanica.postman_collection.json)
   - [Swagger/Scalar UI](http://localhost:5000/scalar) (requer API rodando)
   ```

---

### Item 4 — Terraform para Cloud ❌ (opcional / baixa prioridade)

**Passos (se decidido implementar):**

1. Criar `infra/cloud/aws/` com:
   - `main.tf` — provider AWS + módulo EKS
   - `variables.tf` — região, tamanho dos nodes, nome do cluster
   - `outputs.tf` — kubeconfig, cluster endpoint

2. Usar módulo oficial: `module "eks" { source = "terraform-aws-modules/eks/aws" }`

3. Adicionar RDS PostgreSQL ou adaptar o módulo `infra/modules/postgres/` para usar `aws_db_instance`

4. Documentar em `infra/cloud/aws/README.md` com requisitos de credenciais AWS

5. Adicionar secrets `AWS_ACCESS_KEY_ID` e `AWS_SECRET_ACCESS_KEY` no GitHub para o pipeline

---

## 6. Checklist Final para Submissão

- [ ] Item 1: Implementar link de aprovação no e-mail
- [ ] Item 2: Gravar vídeo e adicionar link no README
- [ ] Item 3: Exportar Postman collection e linkar no README
- [ ] Item 4 (opcional): Terraform Cloud
- [ ] Verificar que o repositório está compartilhado com `soat-architecture` no GitHub
- [ ] Gerar PDF com link do repositório + desenho de arquitetura + link do vídeo para entrega no portal do aluno
