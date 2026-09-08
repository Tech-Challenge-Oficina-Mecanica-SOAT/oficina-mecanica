# Roteiro de apresentação — Fase 3, entrega P1 (PR #44)

Roteiro para gravar o vídeo explicando o que foi entregue na branch `feature/Fase3-P1-Pacheco` (mergeada na `main` via PR #44). Cobre a feature original (observabilidade, auth por CPF, métricas, idempotência, LGPD) mais os ajustes feitos durante a revisão de código do PR.

**Duração estimada:** 12–18 min, dependendo de quanto você narra durante os comandos.

---

## 0. Preparação (antes de gravar, não entra no vídeo)

- [ ] Ambiente AWS Academy com `oficina-infra-db` e `oficina-infra-k8s` de pé (`make up` no `oficina-infra-k8s`, ou já aplicado numa sessão anterior).
- [ ] Repo `oficina-mecanica` na `main`, atualizado (`git pull`).
- [ ] Redis acessível (local via `docker-compose up redis` ou o do cluster).
- [ ] Um cliente REST (Postman/Insomnia/`curl`) com a collection `docs/oficina-mecanica.postman_collection.json` importada.
- [ ] Terminal com fonte grande, sem informações sensíveis visíveis (tokens, credenciais AWS).
- [ ] Abrir o PR #44 no navegador numa aba (para mostrar os comentários de review, se for citar o processo).

---

## 1. Abertura (30s)

Fale, sem tela de código ainda:

> "Esse vídeo cobre a entrega P1 da Fase 3 do Tech Challenge: observabilidade, autenticação de cliente por CPF, métricas de negócio, idempotência em endpoints críticos e os primeiros controles de LGPD. Vou mostrar o código, rodar a API e testar cada peça."

---

## 2. Visão geral da arquitetura (2 min)

Abra `docs/arquitetura-fase3.md` e mostre o diagrama Mermaid renderizado (GitHub renderiza automaticamente).

Pontos a falar:
- 4 repositórios: `oficina-infra-db` (VPC+RDS), `oficina-infra-k8s` (EKS+Redis+manifests), `oficina-mecanica` (a API .NET, foco deste vídeo), `oficina-lambda-auth` (pendente, ainda vazio).
- A API roda dentro do EKS, atrás de um NLB; fala com o RDS (Postgres) e com o Redis (cache de idempotência).
- **Deixe claro o que é real vs. planejado:** o diagrama tem legenda ✅/⏳. A Lambda de auth por CPF e o API Gateway ainda não existem — o que existe do lado da API é o endpoint interno que a Lambda vai chamar quando for implementada.

---

## 3. Autenticação de cliente por CPF (3 min)

### 3.1 Onde está no código

Mostre, em ordem:
1. `src/OficinaMecanica.Domain/ValueObjects/Documento.cs` — value object que valida CPF **e** CNPJ, expõe `Tipo`.
2. `src/OficinaMecanica.Application/UseCases/Auth/AutenticarPorCpf/AutenticarPorCpfUseCase.cs` — o use case. Destaque:
   - Valida o documento e **confere `Tipo == TipoDocumento.Cpf`** (um CNPJ válido não pode logar aqui — isso foi um ajuste feito durante a revisão do PR).
   - Usa `IAutenticacaoClienteQuery` em vez de `IClienteRepository` diretamente — explique rapidamente a disciplina de agregados do `CONTRIBUTING.md`: um use case do agregado `Usuario`/Auth não deve chamar o repositório do agregado `Cliente` diretamente; a query de leitura dedicada é a exceção documentada para esse tipo de consulta síncrona cross-agregado.
3. `src/OficinaMecanica.API/Controllers/InternalAuthController.cs` — o endpoint `POST /internal/auth/cpf-verify`, protegido por API Key (`[Authorize(AuthenticationSchemes = "ApiKey")]`), destinado a ser chamado pela futura Lambda.
4. `src/OficinaMecanica.Infrastructure/Auth/InternalApiKeyProvider.cs` — mostre a comparação da API Key em **tempo constante** (`CryptographicOperations.FixedTimeEquals`), e explique por quê: evita timing attack (um atacante medindo a latência da resposta não consegue descobrir a chave caractere por caractere).

### 3.2 Demonstração ao vivo

Com a API rodando e um cliente + usuário já cadastrados no banco:

```bash
curl -X POST http://localhost:5000/internal/auth/cpf-verify \
  -H "Content-Type: application/json" \
  -H "X-Internal-Api-Key: <sua-chave-configurada>" \
  -d '{"cpf": "<cpf-de-um-cliente-ativo>"}'
```

Mostre:
- **Sucesso:** retorna `200` com o JWT.
- **CPF inexistente:** `404`.
- **Sem a API Key ou com chave errada:** `401`.
- **(Opcional) Mandando um CNPJ válido:** `400 Validation` — reforça o ajuste do tipo de documento.

---

## 4. Idempotência (5 min — é a parte com mais profundidade técnica)

Essa é a peça mais revisada do PR (7 rodadas de comentários), vale explicar a evolução, não só o resultado final.

### 4.1 O problema que resolve

Explique com um exemplo: um cliente aprova um orçamento clicando num link de e-mail. Se o e-mail faz *prefetch* do link, ou o cliente clica duas vezes, sem proteção a ordem de serviço seria processada duas vezes.

### 4.2 Percorra `src/OficinaMecanica.API/Filters/IdempotentAttribute.cs` explicando cada mecanismo:

1. **Reserva atômica via Redis `SET NX`** (`TentarReservarAsync`) — evita a race condition clássica de "ler cache → processar → escrever cache": duas requisições simultâneas com a mesma chave não conseguem as duas "vencer" a reserva.
2. **Hash SHA-256 do corpo da requisição** — se a mesma `Idempotency-Key` for reusada com um payload diferente, a API responde `422` em vez de silenciosamente devolver a resposta antiga.
3. **Cache só de respostas 2xx** — um erro de validação (`400`) não fica preso em cache por 24h; o cliente pode corrigir e reenviar com a mesma chave.
4. **Chave derivada de argumento, não só de header** — mostre `WebhookController.cs`, o `[Idempotent(ChaveDeArgumento = "token")]`: o link de aprovação por e-mail não manda header customizado, então a chave vem do próprio token da URL.
5. **Headers preservados** — a gravação do cache acontece em `OnResultExecutionAsync`, depois que o `Location` (de um `CreatedAtActionResult`, por exemplo) já foi calculado.
6. **Fail-open se o Redis cair** — todo o acesso ao `IIdempotencyStore`, **incluindo a resolução do serviço via DI** (ponto sutil: é aí que a conexão com o Redis é estabelecida), está em `try/catch`. Se o Redis estiver fora do ar, a requisição segue sem deduplicação em vez de virar `500`.
7. **Redis no healthcheck de prontidão** — `RedisHealthCheck.cs`, registrado com a tag `"ready"` em `/health/ready`.

### 4.3 Demonstração ao vivo

```bash
# 1. Primeira chamada com Idempotency-Key
curl -X POST http://localhost:5000/api/ordens-servico \
  -H "Authorization: Bearer <token>" \
  -H "Idempotency-Key: demo-123" \
  -H "Content-Type: application/json" \
  -d '{"clienteId": "...", "veiculoId": "...", "observacoes": "Teste"}'

# 2. Repita EXATAMENTE a mesma chamada — mesma Idempotency-Key, mesmo corpo
#    (deve devolver a MESMA resposta, sem criar uma segunda OS)

# 3. Repita com a MESMA chave mas corpo diferente — deve devolver 422
```

Complementar (se quiser mostrar o fail-open): pare o Redis local (`docker stop <container-redis>`) e repita o passo 1 — a requisição deve continuar funcionando (sem dedup), não deve dar 500.

---

## 5. Healthchecks (1min30)

```bash
curl http://localhost:5000/health         # liveness "puro" — não depende de banco nem Redis
curl http://localhost:5000/health/ready   # prontidão — checa Postgres E Redis
curl http://localhost:5000/health/live    # sempre 200, para o liveness probe do k8s
```

Explique por que separar: um `/health` que depende do banco faria o **liveness probe** do Kubernetes reiniciar o pod à toa numa falha transitória de banco — o pod está vivo, só não está pronto para receber tráfego. Mostre `k8s/api-deployment.yaml` com `readinessProbe` → `/health/ready` e `livenessProbe` → `/health/live`.

---

## 6. Métricas de negócio (2 min)

1. `src/OficinaMecanica.Infrastructure/Metrics/OrdemServicoMetrics.cs` — três métricas Prometheus: contador de OS abertas, gauge de OS por status, histograma de tempo de execução.
2. Mostre os pontos de chamada (`RegistrarAbertura`, `AtualizarStatus`) nos use cases de transição de status da OS.
3. Ao vivo:
   ```bash
   curl http://localhost:5000/metrics | grep -E "os_abertas_total|os_por_status_gauge|tempo_execucao_histogram"
   ```
4. (Opcional, se o New Relic estiver instalado no cluster) mostre o painel do New Relic One recebendo essas métricas/traces.

---

## 7. LGPD — o que já está implementado (2 min)

Abra `docs/adrs/ADR-008-tratamento-dados-pessoais.md` e narre a partir da tabela de status (✅/⏳), sem precisar ler tudo:

1. **Encryption at rest** no RDS (`storage_encrypted = true`) — mostre em `oficina-infra-db/modules/rds/main.tf`.
2. **Mascaramento de CPF em logs** — `src/OficinaMecanica.Infrastructure/Logging/CpfMaskingTextFormatter.cs`. Ao vivo, dispare uma requisição que logue um CPF e mostre no console/log que aparece como `123.***.***-00`, nunca completo.
3. Deixe claro o que **ainda não** está pronto (⏳): registro de consentimento LGPD e TLS forçado no RDS — são dívidas conhecidas, documentadas, não esquecidas.

---

## 8. Panorama do processo de revisão (1min30, opcional mas recomendado)

Abra o PR #44 no navegador, role pelos comentários do revisor. Não precisa ler todos, só apontar o padrão:

> "O PR passou por várias rodadas de revisão — race condition na idempotência, validação de tipo de documento, timing attack na comparação de API Key, resiliência a falha do Redis, healthcheck não cobrindo o Redis. Cada um desses virou um commit de fix separado, e o CI (build, testes, code review automático) rodou a cada ajuste."

Mostre rapidamente `git log --oneline` do PR (os commits `fix:`) como evidência do processo iterativo.

---

## 9. Testes automatizados (1 min)

```bash
dotnet test tests/OficinaMecanica.Tests.Unit/OficinaMecanica.Tests.Unit.csproj
```

Aponte o total de testes passando (408 no momento do merge) e cite pontos cobertos: `AutenticarPorCpfUseCaseTests` (incluindo o caso de CNPJ rejeitado), `CpfMaskingTextFormatterTests`.

Se tiver Docker disponível, rode também a suíte de integração (`OficinaMecanica.Tests.Integration`, usa Testcontainers) e mostre `WebhookControllerTests` passando — é o teste que valida idempotência de ponta a ponta no fluxo de aprovação por e-mail.

---

## 10. Fechamento (30s)

> "Resumindo: entregamos autenticação de cliente por CPF, idempotência resiliente em todos os endpoints de mutação de OS, métricas de negócio expostas via Prometheus, healthchecks separando liveness de readiness, e os primeiros controles de LGPD. O que fica pendente para as próximas entregas está documentado nas ADRs: a Lambda de autenticação, o registro de consentimento LGPD e TLS forçado no RDS."

---

## Checklist rápido do que mostrar em tela (para não esquecer nada durante a gravação)

- [ ] `docs/arquitetura-fase3.md` (diagrama)
- [ ] `Documento.cs` (Tipo Cpf/Cnpj)
- [ ] `AutenticarPorCpfUseCase.cs`
- [ ] `InternalAuthController.cs`
- [ ] `InternalApiKeyProvider.cs` (FixedTimeEquals)
- [ ] Chamada `curl` no `/internal/auth/cpf-verify` (sucesso, 404, 401)
- [ ] `IdempotentAttribute.cs` (percorrer os 7 pontos da seção 4.2)
- [ ] `WebhookController.cs` (`ChaveDeArgumento`)
- [ ] Duas chamadas `POST /api/ordens-servico` com a mesma `Idempotency-Key`
- [ ] `/health`, `/health/ready`, `/health/live`
- [ ] `k8s/api-deployment.yaml` (probes)
- [ ] `OrdemServicoMetrics.cs` + `curl /metrics`
- [ ] `ADR-008` + `CpfMaskingTextFormatter.cs`
- [ ] PR #44 no navegador (comentários de review)
- [ ] `dotnet test` rodando
