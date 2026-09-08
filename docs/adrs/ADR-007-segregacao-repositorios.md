# ADR-007 — Segregação em 4 Repositórios

| Campo        | Valor                          |
|--------------|-------------------------------|
| **Status**   | Aceito, implementado e testado |
| **Data**     | 2026-09-08 (retroativa; estrutura adotada desde o início da Fase 3, formalizada agora) |
| **Autores**  | grupo Tech Challenge Oficina Mecânica |
| **Contexto** | Fase 3 — Tech Challenge FIAP   |

---

## Contexto

O enunciado da Fase 3 exige separação em repositórios distintos para a Lambda, a infraestrutura Kubernetes, a infraestrutura de dados e a aplicação. Era preciso definir como esses repositórios se comunicam e são versionados de forma independente.

---

## Decisão

Adotar **4 repositórios independentes**, com contratos formais entre eles via **AWS Systems Manager Parameter Store** e **Secrets Manager**, nunca por acesso direto ao state Terraform de outro repositório:

| Repositório | Responsabilidade |
|---|---|
| `oficina-mecanica` | API .NET (aplicação principal) |
| `oficina-lambda-auth` | Lambda de autenticação por CPF + API Gateway |
| `oficina-infra-k8s` | Cluster EKS, manifestos Kubernetes, observabilidade |
| `oficina-infra-db` | VPC, RDS PostgreSQL, Secrets Manager |

`oficina-infra-db` é a base: publica os contratos que os outros três consomem, e por isso precisa ser aplicado primeiro.

## Razão

- Cadências independentes: infraestrutura muda pouco, aplicação muda com frequência — separar evita que mudanças de infraestrutura fiquem acopladas a cada release de aplicação.
- Isolamento de blast radius: um erro no pipeline da aplicação não pode derrubar a infraestrutura.
- Ownership claro por repositório.
- Alinha com um padrão comum de organizações que operam infraestrutura e aplicação com times/repositórios separados.

## Alternativas descartadas

- **Monorepo com pipelines separadas**: perderia o objetivo pedagógico do enunciado (que pede repositórios separados de fato).
- **Terraform remote state cruzado** (`terraform_remote_state` lendo o backend de outro repositório): mais acoplado, exige cada repositório conhecer o backend do outro.
- **Git submodules**: complexidade adicional sem benefício real para o tamanho deste projeto.

---

## Contratos publicados (implementação real)

**Parameter Store**, publicados por `oficina-infra-db`, consumidos por `oficina-infra-k8s` e `oficina-lambda-auth`:
```
/oficina/{env}/network/vpc-id
/oficina/{env}/network/vpc-cidr
/oficina/{env}/network/public-subnet-ids
/oficina/{env}/network/private-subnet-ids
/oficina/{env}/db/endpoint
/oficina/{env}/db/port
/oficina/{env}/db/name
/oficina/{env}/db/username
/oficina/{env}/db/security-group-id
```

**Parameter Store**, publicados por `oficina-infra-k8s`, consumidos por `oficina-lambda-auth`:
```
/oficina/{env}/k8s/cluster-name
/oficina/{env}/k8s/cluster-endpoint
/oficina/{env}/k8s/cluster-security-group-id
/oficina/{env}/k8s/api-endpoint
```

**Parameter Store**, publicados por `oficina-lambda-auth`:
```
/oficina/{env}/api-gateway/endpoint
/oficina/{env}/api-gateway/id
```

**Secrets Manager**, publicados por `oficina-infra-db`, compartilhados por todos:
```
oficina/{env}/db-password       → oficina-mecanica, oficina-lambda-auth
oficina/{env}/jwt-secret-key    → oficina-mecanica, oficina-lambda-auth (mesma chave assina/valida o JWT nos dois lados)
```

**GHCR** (não é um contrato AWS): `oficina-mecanica` publica a imagem da API automaticamente a cada merge em `main`; `oficina-infra-k8s` consome essa imagem via `imagePullSecret` (pacote privado).

---

## Descobertas reais durante a implementação (divergências do planejamento original)

### 1. O NLB não pode ser referenciado por um valor publicado estaticamente

O plano original previa publicar o ARN do NLB como um parâmetro fixo no Parameter Store (`/oficina/{env}/k8s/nlb-arn`), para o API Gateway usar na integração `VPC_LINK`. **Isso não funciona na prática**: o NLB é recriado pelo controlador do EKS toda vez que o `Service` do Kubernetes é recriado (destroy/apply do cluster, por exemplo), e o ARN muda a cada recriação — um valor publicado uma vez ficaria obsoleto na primeira vez que a infraestrutura fosse reaplicada.

Resolvido em `oficina-lambda-auth` com um `data "aws_lb"` que busca o NLB **por tag** (`kubernetes.io/service-name`, que o controlador do EKS sempre define), resolvido em tempo de `apply`, nunca lido de um valor publicado estaticamente. O parâmetro `/oficina/{env}/k8s/api-endpoint` continua existindo (hostname do NLB, útil para humanos/scripts de teste), mas não é a fonte usada pela integração do API Gateway.

### 2. `repository_dispatch` entre repositórios nunca foi implementado

O plano original previa que o CI/CD de `oficina-mecanica` acionasse `oficina-infra-k8s` via `repository_dispatch` (GitHub) ao publicar uma nova imagem, para deploy automático. Isso nunca foi construído. Em vez disso, `oficina-infra-k8s` tem um workflow próprio (`deploy-manifests.yml`) disparado manualmente (`workflow_dispatch`), que sempre aplica a imagem mais recente publicada em `latest`. A razão: as credenciais do AWS Academy expiram a cada 4 horas, o que torna um disparo automático entre repositórios frágil (a credencial pode já estar expirada no momento em que o `repository_dispatch` chega). Disparo manual, no mesmo padrão do workflow de `terraform apply`, evita esse problema.

### 3. Nome de repositório incorreto quebrava um workflow silenciosamente

O workflow de migrations (`oficina-infra-db/.github/workflows/migrations.yml`) fazia checkout de um repositório chamado `oficina-mecanica-api` — nome que nunca existiu (o repositório real é `oficina-mecanica`). Esse erro nunca tinha sido percebido porque as execuções anteriores do workflow sempre falhavam antes de chegar nesse passo (credenciais AWS expiradas), mascarando o problema. Corrigido junto com um segundo bug relacionado: o script de migrations lia a senha do banco de um caminho do Secrets Manager com uma barra inicial (`/oficina/{env}/db-password`) que não corresponde ao nome real do secret (`oficina/{env}/db-password`, sem a barra) e exportava a connection string sob um nome de variável (`ConnectionStrings__DefaultConnection`) diferente do que a ferramenta de design-time do EF Core realmente lê (`DEFAULT_CONNECTION`). **Lição:** um contrato entre repositórios (nome de repositório, caminho de secret, nome de variável de ambiente) só é validado de verdade rodando o fluxo completo, não lendo o código isoladamente.

---

## Consequências

**Positivas:**
- Contratos explícitos e versionados (Parameter Store/Secrets Manager), fáceis de auditar sem precisar ler o código de outro repositório.
- As duas divergências do plano original (busca por tag em vez de ARN estático, `workflow_dispatch` em vez de `repository_dispatch`) resultaram em soluções mais robustas que as originalmente propostas.

**Negativas:**
- Novo integrante precisa clonar até 4 repositórios para ter visão completa do sistema.
- Contratos "silenciosos" (nome de repositório, nome de variável de ambiente) só quebram em tempo de execução, não em tempo de escrita do código — exige testar o fluxo completo periodicamente, não só cada repositório isoladamente.

---

## Referências

- READMEs de cada um dos 4 repositórios, seção "Contratos publicados"/"Dependências"
- `oficina-lambda-auth/terraform/modules/api-gateway/main.tf` — resolução do NLB por tag
- `oficina-infra-k8s/.github/workflows/deploy-manifests.yml` — deploy manual em vez de `repository_dispatch`
- [ADR-004](./ADR-004-api-gateway-pattern.md) — API Gateway que depende da resolução do NLB por tag
