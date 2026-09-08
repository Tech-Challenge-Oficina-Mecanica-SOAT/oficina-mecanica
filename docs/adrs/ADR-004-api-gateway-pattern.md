# ADR-004 — API Gateway Pattern

| Campo        | Valor                          |
|--------------|-------------------------------|
| **Status**   | Aceito, implementado e testado |
| **Data**     | 2026-09-08 (retroativa; decisão tomada e implementada em `oficina-lambda-auth` antes desta data, formalizada agora) |
| **Autores**  | grupo Tech Challenge Oficina Mecânica |
| **Contexto** | Fase 3 — Tech Challenge FIAP   |

---

## Contexto

A Fase 3 exige um API Gateway para expor a Lambda de autenticação por CPF e rotear tráfego até a API principal rodando no EKS. Era preciso escolher entre AWS API Gateway (REST API vs HTTP API), Application Load Balancer, ou uma solução self-hosted (Kong, Traefik).

---

## Decisão

Usar **AWS API Gateway HTTP API (v2)**, implementado em `oficina-lambda-auth/terraform/modules/api-gateway`.

---

## Implementação real

- `aws_apigatewayv2_api` com `protocol_type = "HTTP"` e CORS configurado (`allow_origins = ["*"]`).
- `POST /auth/cpf` → integração `AWS_PROXY` direto com a Lambda (payload format 2.0), sem VPC Link (API Gateway integra nativamente com Lambda).
- `ANY /api/{proxy+}` e `ANY /publico/{proxy+}` → integração `HTTP_PROXY` sobre **VPC Link**, apontando para o NLB do EKS.
- Autorização JWT nativa do API Gateway **não é usada** — a API .NET valida o token Bearer em cada request, como já fazia para o fluxo de login tradicional. Mantém a validação num único lugar, independente da origem do token (login direto ou Lambda).
- `aws_cloudwatch_log_group` para logs de acesso do stage `$default` (`auto_deploy = true`).

---

## Descoberta real durante a implementação (não prevista no planejamento)

A integração `HTTP_PROXY` sobre `VPC_LINK` **exige o ARN do listener do NLB** em `integration_uri`, não aceita hostname/URL (`http://<endpoint>/{proxy}` é rejeitado com `BadRequestException: integration uri should be a valid ELB listener ARN`). Isso só foi descoberto testando a integração de ponta a ponta pela primeira vez (ver ADR-007 para como o ARN é resolvido sem publicar um valor estático que ficaria obsoleto).

Também foi necessário atribuir um Security Group explícito ao `aws_apigatewayv2_vpc_link` — o Security Group default da VPC não tem nenhuma regra de ingresso/egresso, e sem um SG que libere o tráfego até o NLB a integração falha com `503 Service Unavailable`. Resolvido reaproveitando o Security Group do próprio cluster EKS.

---

## Razão

- Integração nativa com Lambda, sem VPC Link para essa rota.
- Integração com serviços privados no EKS via VPC Link + NLB, mantendo o tráfego dentro da VPC.
- Custo por requisição ~70% menor que REST API (v1).
- Latência menor que REST API para requests simples.
- Suficiente para os requisitos do projeto — não são necessárias features exclusivas de REST API (request/response transformation, usage plans complexos).

## Alternativas descartadas

- **REST API (API Gateway v1)**: mais caro, mais complexo, features desnecessárias para este projeto.
- **ALB direto**: não integra nativamente com Lambda (exigiria um target group tipo Lambda, mais verboso e sem os outros benefícios do API Gateway).
- **Kong/Traefik self-hosted**: exigiria manter mais um serviço no cluster, overhead operacional sem benefício real no contexto do AWS Academy.

---

## Consequências

**Positivas:**
- Uma única porta de entrada pública para Lambda e API, com CORS e logs de acesso centralizados.
- Testado de ponta a ponta: `POST /auth/cpf` retorna um JWT válido, e o proxy `/api/*` entrega esse mesmo JWT até a API .NET no EKS, que o valida corretamente (confirmado com um teste real: token de um cliente sendo negado com 403 ao tentar acessar um endpoint exclusivo de Admin, prova que o token atravessa toda a cadeia).

**Negativas:**
- O comportamento do `integration_uri` para VPC Link (exigir ARN de listener, não URL) não é intuitivo e só foi descoberto na prática; documentado aqui e no código para não ser redescoberto.
- VPC Link tem tempo de provisionamento alto (~2 minutos), o que torna qualquer `terraform apply` que recrie esse recurso mais lento.

---

## Referências

- `oficina-lambda-auth/terraform/modules/api-gateway/main.tf`
- [ADR-005](./ADR-005-lambda-para-autenticacao.md) — Lambda que essa API Gateway expõe
- [ADR-007](./ADR-007-segregacao-repositorios.md) — como o ARN do NLB é resolvido sem publicar um valor estático
