# Roteiro de apresentação (4 min) — Entregas P1 Fase 3 — Rafael Pacheco

Roteiro enxuto para vídeo de **até 4 minutos**, cobrindo **somente os itens sob responsabilidade P1 (Rafael Pacheco)** na planilha de acompanhamento (`Pos - Fase 1 x Fase 2 x Fase 3.xlsx`) **que já foram efetivamente entregues** na branch `feature/Fase3-P1-Pacheco` (PR #44, mergeada na `main`) e nos ajustes subsequentes (PRs #46, #47, #48, #49).

A planilha listava os itens 13–15 com `Repo: oficina-mecanica-api`, mas na prática **13 e 14 foram implementados nos repositórios de infraestrutura** (`oficina-infra-k8s` e `oficina-infra-db`), não no repo da API — por isso não apareciam nos commits deste PR. O item 15 (README) segue apenas parcial. Como o vídeo é uma demo ao vivo focada no repo `oficina-mecanica`, a demonstração de tela continua restrita ao que roda nessa API; 13 e 14 entram só como menção falada, já que a evidência está em outro repositório.

---

## Checklist — item da planilha (P1) → status → requisito do PDF Tech Challenge

| # | Item (planilha, Responsável P1) | Status | Onde está no código | Requisito correspondente no PDF do Tech Challenge |
|---|---|---|---|---|
| 1 | Use case `AutenticarPorCpfUseCase` | ✅ Entregue | `Application/UseCases/Auth/AutenticarPorCpf/AutenticarPorCpfUseCase.cs` | *Autenticação e API Gateway* → Function Serverless deve "validar o CPF" e "consultar a existência e o status do cliente na base de dados" |
| 2 | Endpoint interno `/internal/auth/cpf-verify` | ✅ Entregue | `API/Controllers/InternalAuthController.cs` + `Infrastructure/Auth/InternalApiKeyProvider.cs` | *Autenticação e API Gateway* → ponte entre a Function Serverless e a API principal para consulta do cliente |
| 3 | Aceitar issuer da Lambda de auth no JWT | ✅ Entregue (PR #46) | validação de JWT na API aceitando o `issuer` emitido pela Lambda de CPF | *Autenticação e API Gateway* → Function Serverless "gera e devolve um token (JWT) válido para consumo das APIs protegidas" |
| 4 | Instrumentação New Relic | ✅ Entregue | agente New Relic configurado no projeto da API | *Monitoramento e Observabilidade* → integração com Datadog/New Relic |
| 5 | Logs estruturados JSON + correlationId (W3C traceparent) | ✅ Entregue | `Infrastructure/Logging/CorrelationIdEnricher.cs` (usa `Activity.Current.TraceId`) | *Monitoramento e Observabilidade* → "logs estruturados (JSON), incluindo correlação entre requisições" |
| 6 | Healthchecks avançados (`/health`, `/health/ready`, `/health/live`) | ✅ Entregue | `Program.cs` (`MapHealthChecks`), com fix posterior para não derrubar o liveness por causa do banco | *Monitoramento e Observabilidade* → "healthchecks e uptime" |
| 7 | Métricas customizadas de negócio | ✅ Entregue | `Infrastructure/Metrics/OrdemServicoMetrics.cs` (`/metrics` Prometheus: OS abertas, OS por status, tempo de execução) | *Monitoramento e Observabilidade* → "volume diário de OS", "tempo médio de execução por status", dashboards |
| 8 | Idempotency-Key middleware | ✅ Entregue | `API/Filters/IdempotentAttribute.cs` + `Infrastructure/Idempotency/RedisIdempotencyStore.cs`, com fail-open se o Redis cair | Não é requisito literal do PDF, mas sustenta *"alertas para falhas no processamento de ordens de serviço"* — evita duplicidade de OS |
| 9 | Isolamento de falha de handler de evento de domínio | ✅ Entregue (fix posterior) | `DomainEventDispatcher` agora isola cada handler em try/catch | *Monitoramento e Observabilidade* → confiabilidade do processamento de OS (efeito colateral com falha não derruba a operação principal) |
| 10 | Mascaramento de CPF em logs (LGPD) | ✅ Entregue | `Infrastructure/Logging/CpfMaskingTextFormatter.cs` | Não é requisito explícito do Tech Challenge Fase 3, mas está documentado como preparação (ADR-008) |
| 11 | Disciplina de agregados (Domain Events só com dados primitivos) | ✅ Entregue | `CONTRIBUTING.md` + auditoria dos `Domain/Events` | Suporte à *Documentação da Arquitetura* (ADRs de decisões arquiteturais) |
| 12 | Endpoint faltante `EmDiagnostico → AguardandoAprovacao` | ✅ Entregue (fix posterior) | endpoint adicionado no controller de status de OS | *Monitoramento e Observabilidade* → fluxo completo de status da OS sem lacunas (Diagnóstico → Execução → Finalização) |
| 13 | CI/CD para ECR + trigger de deploy | 🔀 Entregue diferente do planejado | `oficina-mecanica/.github/workflows/ci.yml` publica no **GHCR** (não ECR — decisão registrada em ADR); `oficina-infra-k8s` consome a imagem via PAT read-only, sem trigger automático de repository_dispatch | *Estrutura de Repositórios e CI/CD* → "deploy automático para a nuvem" |
| 14 | Ambientes homolog/produção | ✅ Entregue (em outro repositório) | `oficina-infra-k8s/terraform/envs/{homolog,prod}` + `oficina-infra-db/envs/{homolog,prod}`, cada um com workflow `apply.yml` via `workflow_dispatch` e GitHub Environment (approval manual em prod) | *Estrutura de Repositórios e CI/CD* → "deploy automático das branches de homologação e produção" |
| 15 | README focado em Aplicação | 🟡 Parcial | `oficina-mecanica/README.md` já cobre CI/CD, Postman e Scalar, mas ainda não linka os outros 3 repositórios (`oficina-infra-k8s`, `oficina-infra-db`, `oficina-lambda-auth`) nem a execução via docker-compose apontando pra eles | *README.md em cada repositório* → "passos para execução e deploy" |

---

## Roteiro (fala + tela) — 4 minutos

### 0:00 – 0:15 | Abertura
> "Este vídeo cobre as entregas de responsabilidade P1 da Fase 3 do Tech Challenge, feitas por mim, Rafael Pacheco, na branch `feature/Fase3-P1-Pacheco`: autenticação de cliente por CPF integrada à Lambda, observabilidade completa, idempotência e os primeiros controles de LGPD."

### 0:15 – 1:15 | Autenticação por CPF (integração com a Lambda) — 60s
Mostre em sequência, rápido:
- `AutenticarPorCpfUseCase.cs` — valida CPF, confere status do cliente.
- `InternalAuthController.cs` — endpoint `POST /internal/auth/cpf-verify` protegido por API Key em tempo constante.
- Explique em uma frase que a validação de JWT da API foi ajustada para **aceitar o issuer emitido pela Lambda de autenticação por CPF**, fechando o ciclo: Lambda valida CPF → gera JWT → API principal aceita esse token nas rotas protegidas.

> "Aqui está o requisito de autenticação via CPF do desafio: a Function Serverless valida o CPF, consulta o status do cliente através desse endpoint interno, e o JWT que ela emite já é aceito nativamente pela API."

### 1:15 – 2:25 | Observabilidade — 70s
Mostre rapidamente, um `curl` para cada:
```bash
curl http://localhost:5000/health/ready
curl http://localhost:5000/metrics | grep os_
```
- Logs JSON com `CorrelationId` (padrão W3C `traceparent`, via `Activity.Current`).
- Healthchecks separados: `/health/live` (liveness) e `/health/ready` (checa Postgres e Redis).
- Métricas Prometheus de negócio: OS abertas, OS por status, tempo de execução — a base dos dashboards pedidos no desafio.
- Agente New Relic instrumentando a aplicação.

> "Isso cobre latência, uptime, e os dashboards de volume e tempo médio de execução por status exigidos no desafio."

### 2:25 – 3:15 | Idempotência e resiliência — 50s
- `IdempotentAttribute.cs`: reserva atômica via Redis `SET NX`, hash do corpo para detectar reuso indevido de chave, fail-open se o Redis cair.
- Uma frase sobre o `DomainEventDispatcher`: falha num handler de evento (ex.: SMTP fora do ar) não derruba mais a operação principal — a OS já foi salva, o efeito colateral falho só é logado.

> "Isso garante que uma ordem de serviço não é processada em duplicidade e que uma falha de e-mail não vira um erro 500 para o cliente."

### 3:15 – 3:45 | LGPD — 30s
- `CpfMaskingTextFormatter.cs`: CPF nunca aparece completo em log, sempre mascarado.

> "Primeiro controle de LGPD já em produção: nenhum log expõe o CPF completo do cliente."

### 3:45 – 4:00 | Fechamento — 15s
> "Resumindo: autenticação por CPF integrada à Lambda, observabilidade completa com logs, métricas e healthchecks, idempotência resiliente, e mascaramento de dados sensíveis em log. Os ambientes de homolog e produção e o build da imagem já estão entregues nos repositórios de infraestrutura; o que falta neste pacote é só fechar o README da API com os links pros outros repositórios."

---

## Checklist rápido para gravação

- [ ] `AutenticarPorCpfUseCase.cs`
- [ ] `InternalAuthController.cs` (curl de sucesso)
- [ ] Menção rápida ao issuer da Lambda aceito no JWT
- [ ] `curl /health/ready`
- [ ] `curl /metrics | grep os_`
- [ ] `CorrelationIdEnricher.cs` (ou um log JSON com CorrelationId)
- [ ] `IdempotentAttribute.cs` (SET NX + fail-open, sem repetir a demo completa dos 7 pontos)
- [ ] Frase sobre isolamento de handler de evento de domínio
- [ ] `CpfMaskingTextFormatter.cs`
