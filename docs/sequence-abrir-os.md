# Diagrama de sequência — Abertura de Ordem de Serviço

Reflete o fluxo real implementado em `OrdemServicosController.Create` → `AbrirOrdemServicoUseCase` (branch do PR #44). Diferente do que o plano-05 original assumia, esta versão **não** dispara Domain Event nem envia email ao abrir a OS; a implementação atual só persiste a ordem e registra uma métrica.

```mermaid
sequenceDiagram
    autonumber
    actor A as Admin/Mecânico
    participant NLB as NLB EKS
    participant API as API .NET (Pod)
    participant IF as IdempotentAttribute
    participant R as Redis (IIdempotencyStore)
    participant UC as AbrirOrdemServicoUseCase
    participant DB as RDS Postgres
    participant M as OrdemServicoMetrics

    A->>NLB: POST /api/ordens-servico<br/>Bearer JWT, Idempotency-Key (opcional)
    NLB->>API: HTTP forward

    API->>API: UseAuthentication/UseAuthorization<br/>valida JWT, exige role Admin

    API->>IF: OnActionExecutionAsync
    IF->>R: ObterAsync(idempotency:{path}:{key})

    alt Idempotency-Key já usada antes
        R-->>IF: resposta em cache
        IF-->>NLB: replay da resposta cacheada<br/>(mesmo status code e corpo originais)
        NLB-->>A: resposta cacheada
    else Sem cache (primeira vez ou sem header)
        R-->>IF: null
        IF->>UC: next() → Executar(request)

        UC->>UC: Valida ClienteId e VeiculoId não vazios

        alt Request inválido
            UC-->>API: Result.Validation
            API-->>IF: 400 Bad Request
            IF->>R: SalvarAsync(chave, resposta, 24h)
            IF-->>NLB: 400
        else Request válido
            UC->>DB: INSERT ordem_servico (status=Recebida)
            DB-->>UC: id (Guid)

            UC->>DB: SELECT ordem_servico com Itens WHERE id
            DB-->>UC: ordem completa

            UC->>M: RegistrarAbertura()

            UC-->>API: Result.Success(response)
            API-->>IF: 201 Created
            IF->>R: SalvarAsync(chave, resposta, 24h)
            IF-->>NLB: 201 Created
        end
    end

    NLB-->>A: 200/201/400 conforme o caso

    Note over A,M: Traces, logs, métricas → New Relic<br/>via agentes DaemonSet no cluster
```

## Diferenças em relação ao plano original

- Não existe `Domain Event Dispatcher` nem `EnviarEmailHandler`/SMTP na abertura de OS hoje; a plano-05 assumia notificação por email nesse fluxo, mas o código atual só persiste e mede.
- A rota não passa por API Gateway hoje (vai direto ao NLB). Se isso mudar no futuro, é decisão pendente de P2 (ver `docs/arquitetura-fase3.md`).
- O `[Idempotent]` também cacheia respostas de erro (ex: 400), não só sucessos. Isso foi levantado como achado de review no PR #44 (uma segunda tentativa com a mesma `Idempotency-Key` depois de corrigir o request continua recebendo o erro antigo em cache por até 24h).

## Pendências

- A infraestrutura de deploy (namespace, Service/NLB, Redis, HPA, ConfigMap) já foi testada de ponta a ponta contra um cluster EKS real (ver `checklist-p3-oficina-infra-k8s.md` no repositório `oficina-infra-k8s`). O fluxo de negócio descrito acima ainda não foi exercitado contra um pod real da API, porque o `Deployment` usa a imagem placeholder `<ECR_URL>` e nunca ficou `Ready`; falta testar de ponta a ponta assim que a imagem real for publicada no ECR.
