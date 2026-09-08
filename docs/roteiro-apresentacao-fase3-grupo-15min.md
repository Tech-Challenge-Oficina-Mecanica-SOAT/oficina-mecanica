# Roteiro de apresentação em grupo (até 15 min) — Tech Challenge Fase 3

Roteiro para o vídeo de demonstração exigido pelo desafio (upload no YouTube/Vimeo, público ou não listado, **até 15 minutos**), dividido entre **4 integrantes**, cobrindo todos os requisitos obrigatórios do PDF do Tech Challenge Fase 3 e os quatro repositórios do projeto:

1. [`oficina-infra-db`](https://github.com/Tech-Challenge-Oficina-Mecanica-SOAT/oficina-infra-db) — banco de dados gerenciado (Terraform).
2. [`oficina-infra-k8s`](https://github.com/Tech-Challenge-Oficina-Mecanica-SOAT/oficina-infra-k8s) — cluster EKS e manifestos Kubernetes (Terraform).
3. [`oficina-mecanica`](https://github.com/Tech-Challenge-Oficina-Mecanica-SOAT/oficina-mecanica) — aplicação principal (API .NET).
4. [`oficina-lambda-auth`](https://github.com/Tech-Challenge-Oficina-Mecanica-SOAT/oficina-lambda-auth) — Function Serverless de autenticação por CPF.

A divisão de blocos abaixo segue as seções do PDF do desafio. **Os nomes ficam em aberto** — o grupo decide quem fala em cada bloco; mantenha a ordem e os tempos para caber nos 15 minutos.

> Cada integrante deve trocar de tela para o repositório que está demonstrando no momento certo (indicado em cada bloco). Testem o roteiro com cronômetro antes de gravar — ele já tem ~30s de folga por bloco para imprevistos.

---

## Visão geral dos blocos

| Bloco | Tempo | Foco (seção do PDF) | Repositório(s) em tela |
|---|---|---|---|
| Abertura | 0:00 – 0:30 | Contexto do desafio e dos 4 repositórios | — (slide/diagrama) |
| **Integrante 1** | 0:30 – 4:15 | Autenticação e API Gateway (CPF → JWT) | `oficina-lambda-auth`, `oficina-mecanica` |
| **Integrante 2** | 4:15 – 8:00 | Infraestrutura: Terraform, banco gerenciado, cluster Kubernetes, CI/CD dos 4 repositórios | `oficina-infra-db`, `oficina-infra-k8s` |
| **Integrante 3** | 8:00 – 11:45 | Monitoramento e Observabilidade | `oficina-mecanica` (New Relic, logs, métricas) |
| **Integrante 4** | 11:45 – 14:45 | Documentação da arquitetura + consumo das APIs protegidas | Documentos + Scalar/Postman |
| Fechamento | 14:45 – 15:00 | Encerramento | — |

Tempo total: **15:00**.

---

## Abertura (0:00 – 0:30) — todos ou um só integrante

> "Este vídeo demonstra a entrega da Fase 3 do Tech Challenge da FIAP SOAT: elevamos a aplicação de oficina mecânica a um nível de operação corporativa, com API Gateway e autenticação serverless, banco de dados gerenciado, cluster Kubernetes escalável na AWS, observabilidade com New Relic e quatro repositórios segregados com CI/CD completo. Vamos passar por cada parte."

Mostre rapidamente (slide ou o diagrama do repositório): [`oficina-mecanica/docs/arquitetura-fase3.md`](./arquitetura-fase3.md) — visão macro dos quatro repositórios e como se conectam na AWS.

---

## Bloco 1 (0:30 – 4:15) — Autenticação e API Gateway — Integrante 1

**Cobre:** *Autenticação e API Gateway* (API Gateway, autenticação via CPF, Function Serverless que valida CPF/consulta cliente/gera JWT).

1. **(45s) API Gateway + Lambda** — em `oficina-lambda-auth/README.md`, mostre o diagrama de arquitetura: API Gateway HTTP API roteando `POST /auth/cpf` para a Lambda, e as rotas `/api/{proxy+}` e `/publico/{proxy+}` passando por VPC Link + NLB até o EKS.
   > "O API Gateway é a porta de entrada única: autentica o cliente por CPF via Lambda e também roteia o restante do tráfego para a API dentro da VPC, sem expor o cluster diretamente à internet."

2. **(60s) A Function Serverless** — abra o código da Lambda (`oficina-lambda-auth/src`): valida os dígitos verificadores do CPF, busca segredos no SSM/Secrets Manager, e chama a API principal.
   > "A Lambda não acessa o banco diretamente nem gera o token sozinha. Ela valida o formato do CPF e delega a consulta do cliente e a emissão do JWT para a API principal — evitando duplicar lógica de autenticação em duas linguagens."

3. **(60s) Ponte com a API principal** — troque para `oficina-mecanica`, abra `API/Controllers/InternalAuthController.cs` e `Application/UseCases/Auth/AutenticarPorCpfUseCase.cs`.
   > "Esse é o endpoint interno `POST /internal/auth/cpf-verify`, protegido por uma API Key própria — só a Lambda o chama. Ele valida o CPF, consulta a existência e o status do cliente no banco, e gera o JWT reaproveitando o mesmo gerador de token usado no login de Admin e Mecânico."

4. **(45s) Demo ao vivo** — `curl` (ou Postman) chamando o fluxo completo:
   ```bash
   curl -X POST https://<api-gateway-url>/auth/cpf -d '{"cpf":"12345678900"}'
   ```
   Mostre o JWT retornado e depois um `curl` numa rota protegida da API usando esse token.
   > "CPF validado, cliente consultado no banco, e o token já sai pronto para consumir as rotas protegidas da API — fechando o requisito de autenticação via CPF do desafio."

*(Reserva: 25s)*

---

## Bloco 2 (4:15 – 8:00) — Infraestrutura, Terraform e CI/CD — Integrante 2

**Cobre:** *Estrutura de Repositórios e CI/CD* (4 repositórios, proteção de branch, PR obrigatório, deploy automático) e *Infraestrutura obrigatória* (banco gerenciado, cluster Kubernetes escalável, Terraform).

1. **(45s) Banco de dados gerenciado** — em `oficina-infra-db`, mostre a estrutura Terraform (VPC, RDS PostgreSQL, Secrets Manager) e o `README.md`.
   > "Este repositório provisiona a VPC, o RDS PostgreSQL gerenciado com storage criptografado, e publica os dados de conexão via Parameter Store e Secrets Manager para os outros repositórios consumirem."

2. **(60s) Cluster Kubernetes escalável** — troque para `oficina-infra-k8s`, mostre `terraform/modules/eks` e o HPA em `k8s/services/api/`.
   > "Aqui está o cluster EKS gerenciado por Terraform, com managed node group e HPA configurado para escalar a API automaticamente por CPU/memória. A regra de security group libera só o tráfego necessário do EKS para o RDS."

3. **(60s) CI/CD e proteção de branch** — mostre `.github/workflows/` de qualquer um dos repositórios (plan automático em PR, apply/deploy manual) e a tela de configuração de branch protection da `main` no GitHub.
   > "Todos os quatro repositórios seguem o mesmo padrão: `main` protegida sem commits diretos, merge só via Pull Request, e pipeline de CI/CD com GitHub Actions. O plano do Terraform roda automaticamente em cada PR; o apply e o deploy são disparos manuais controlados, porque dependem de credenciais temporárias da conta AWS Academy."

4. **(45s) Deploy automático da aplicação** — mostre o workflow `deploy-manifests.yml` do `oficina-infra-k8s` e o `ci.yml` do `oficina-mecanica` que publica a imagem no GHCR.
   > "A `oficina-mecanica` builda a API, roda os testes e publica a imagem automaticamente a cada push na `main`. O `oficina-infra-k8s` consome essa imagem e aplica os manifestos no cluster — fechando o ciclo de deploy automatizado para a nuvem."

*(Reserva: 15s)*

---

## Bloco 3 (8:00 – 11:45) — Monitoramento e Observabilidade — Integrante 3

**Cobre:** *Monitoramento e Observabilidade* (latência, consumo de recursos, healthchecks/uptime, alertas, logs estruturados com correlação, dashboards).

1. **(45s) New Relic instalado no cluster** — mostre `helm/values-newrelic.yaml` (`oficina-infra-k8s`) e o dashboard do New Relic One com o cluster e os pods.
   > "O agente New Relic roda como DaemonSet no cluster, coletando métricas de CPU e memória de cada pod e node, além de traces e logs da aplicação."

2. **(60s) Healthchecks e métricas de negócio** — na `oficina-mecanica`, rode ao vivo:
   ```bash
   curl http://localhost:5000/health/ready
   curl http://localhost:5000/health/live
   curl http://localhost:5000/metrics | grep os_
   ```
   > "Healthchecks separados de liveness e readiness cobrem o uptime. E aqui estão as métricas customizadas de negócio: ordens de serviço abertas, por status, e tempo médio de execução — a base para os dashboards de volume diário e tempo por status exigidos no desafio."

3. **(45s) Logs estruturados com correlação** — mostre um trecho de log JSON com `CorrelationId`/`traceparent` (`Infrastructure/Logging/CorrelationIdEnricher.cs`).
   > "Todo log sai em JSON, correlacionado por request via o padrão W3C traceparent — dá pra seguir uma requisição inteira, do API Gateway até o banco, num único trace."

4. **(45s) Alertas para falhas** — mostre a configuração de alerta no New Relic (ou a lógica de fail-open/isolamento de handler no código, `DomainEventDispatcher`) e explique o cenário coberto.
   > "Configuramos alertas para falha no processamento de ordens de serviço, e no código isolamos cada handler de evento de domínio: uma falha de e-mail, por exemplo, não derruba mais a operação principal de abrir ou aprovar uma OS."

*(Reserva: 30s)*

---

## Bloco 4 (11:45 – 14:45) — Documentação da arquitetura e consumo das APIs — Integrante 4

**Cobre:** *Documentação da Arquitetura* (diagrama de componentes, diagrama de sequência, RFCs, ADRs, justificativa do banco de dados) e evidencia o *consumo das APIs protegidas* via Scalar/Postman.

1. **(45s) Diagrama de componentes e de sequência** — mostre [`docs/arquitetura-fase3.md`](./arquitetura-fase3.md) (visão de nuvem, API Gateway, banco, monitoramento) e [`docs/sequence-auth-cpf.md`](./sequence-auth-cpf.md) / [`docs/sequence-abrir-os.md`](./sequence-abrir-os.md).
   > "Aqui está o diagrama de componentes com a visão completa de nuvem — API Gateway, Lambda, EKS, RDS e New Relic — e os diagramas de sequência do fluxo de autenticação por CPF e de abertura de uma ordem de serviço."

2. **(45s) ADRs e RFCs** — abra a pasta [`docs/adrs/`](./adrs/), destaque 1–2 decisões relevantes (ex: `ADR-001-autenticacao-jwt.md`, decisão de usar GHCR em vez de ECR).
   > "Cada decisão arquitetural relevante — como o padrão de comunicação entre a Lambda e a API, ou o uso de HPA — está registrada como ADR. As decisões técnicas mais amplas, como escolha de nuvem e de banco, estão documentadas como RFC."

3. **(45s) Modelagem do banco de dados** — mostre o diagrama ER (se houver em `docs/` ou `readmeDB.md`) e a justificativa da escolha do PostgreSQL gerenciado.
   > "O modelo relacional foi revisado nesta fase para garantir consistência e performance, com a justificativa formal da escolha do PostgreSQL gerenciado e os relacionamentos documentados no diagrama ER."

4. **(45s) Consumo das APIs protegidas** — abra o Scalar (`http://localhost:5000/scalar`) ou a collection do Postman ([`docs/oficina-mecanica.postman_collection.json`](./oficina-mecanica.postman_collection.json)) e faça uma chamada autenticada com o JWT obtido no Bloco 1.
   > "E aqui fechamos o ciclo: usando o token emitido no início do vídeo, conseguimos consumir as rotas protegidas da API principal, documentadas de ponta a ponta no Scalar."

*(Reserva: 30s)*

---

## Fechamento (14:45 – 15:00) — todos ou um só integrante

> "Resumindo: autenticação via CPF com API Gateway e Function Serverless, infraestrutura como código com Terraform para banco gerenciado e cluster Kubernetes escalável, observabilidade completa com New Relic, logs correlacionados e métricas de negócio, e documentação arquitetural completa com ADRs, RFCs e diagramas. Os quatro repositórios estão linkados na descrição do vídeo. Obrigado."

---

## Checklist de gravação (itens obrigatórios do PDF cobertos)

- [ ] Autenticação com CPF (Bloco 1)
- [ ] Execução da pipeline CI/CD (Bloco 2)
- [ ] Deploy automatizado (Bloco 2)
- [ ] Consumo das APIs protegidas (Bloco 4)
- [ ] Dashboard de monitoramento com análise ao vivo (Bloco 3)
- [ ] Logs e traces em execução (Bloco 3)
- [ ] Diagrama de Componentes e de Sequência (Bloco 4)
- [ ] RFCs e ADRs (Bloco 4)
- [ ] Justificativa do banco de dados e modelo ER (Bloco 4)
- [ ] Menção aos 4 repositórios e à proteção de branch/PR obrigatório (Bloco 2)

## Antes de gravar

- Confirme que o usuário **`soat-architecture`** foi adicionado com acesso aos 4 repositórios (exigido na entrega do Portal do Aluno).
- Tenha o cluster EKS de homolog no ar (não o Kind local) para a demo de infraestrutura e monitoramento refletir o ambiente real da Fase 3.
- Gere um JWT válido via Lambda **antes** de começar a gravar o Bloco 4, para não perder tempo esperando a Lambda no meio da demo — ou grave o Bloco 1 e o Bloco 4 em sequência.
