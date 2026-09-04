# ADR-009 — Escolhas para o Ambiente AWS Academy Learner Lab

| Campo        | Valor                          |
|--------------|-------------------------------|
| **Status**   | Aceito                         |
| **Data**     | 2026-08-27                     |
| **Autores**  | grupo Tech Challenge Oficina Mecânica |
| **Contexto** | Fase 3 — Tech Challenge FIAP   |

---

## Contexto

O projeto roda no AWS Academy Learner Lab fornecido pela FIAP. Esse ambiente impõe restrições reais de IAM, rede e orçamento que moldaram várias decisões de arquitetura, algumas delas descobertas só durante a implementação, não previstas no planejamento original. Esta ADR documenta as limitações, as escolhas feitas por causa delas e como o projeto evoluiria numa conta AWS sem essas restrições.

---

## Limitações do ambiente

### Orçamento

US$50 por conta (Learner Lab tradicional). Se exceder, a conta é desabilitada e todo o trabalho é perdido. Por isso a rotina de destruir a infra ao final de cada sessão de testes (ver `oficina-infra-k8s/README.md`).

### IAM (a restrição mais impactante)

- Não é possível criar roles IAM próprias. Só as pré-criadas pelo Academy estão disponíveis: `LabRole`, `LabEksClusterRole`, `LabEksNodeRole`.
- Os nomes reais dessas roles têm sufixo aleatório gerado por CloudFormation, diferente por conta. O Terraform busca o nome real via `data "aws_iam_roles" { name_regex = "..." }`, nunca por nome literal.
- Sem OIDC provider próprio, portanto sem IRSA (IAM Roles for Service Accounts) no EKS.
- Sem federação, portanto GitHub Actions OIDC não funciona; o CI/CD usa credenciais temporárias exportadas manualmente.
- **Achado central do projeto:** o módulo comunitário `terraform-aws-modules/eks/aws` (testado em v20.37.2 e v21.25.0) chama `iam:GetRole` internamente para resolver a role da sessão STS atual, independentemente das variáveis passadas ao módulo. Essa chamada é negada pela política do Academy (`Pvoclabs2`) sobre a role de sessão `voclabs`, quebrando `plan`/`apply` em qualquer conta Academy. A solução foi usar recursos diretos do provider (`aws_eks_cluster`, `aws_eks_node_group`) em vez do módulo, com `access_config.bootstrap_cluster_creator_admin_permissions = true` para dar acesso ao criador do cluster sem introspecção de IAM.

### Rede

Região fixa em `us-east-1`. VPC customizada é permitida. Route 53 não permite registrar domínios.

### Compute

EKS usa a `LabEksClusterRole` pré-criada. Limite de instâncias EC2 concorrentes do Academy não chegou a ser um problema neste projeto (1 node t3.small).

### Dados

- RDS: só instâncias burstable até `medium`, storage `gp2` até 100GB.
- Multi-AZ não é permitido.
- Enhanced Monitoring não é suportado.

### Sessão

Credenciais expiram a cada 4 horas junto com a sessão. Recursos continuam existindo e cobrando entre sessões (NAT Gateway, RDS), mesmo com a sessão do Academy encerrada — só terminam se forem destruídos explicitamente.

---

## Escolhas conscientes de custo

| Item | Escolha no Academy | Escolha em produção real |
|---|---|---|
| RDS Multi-AZ | Desabilitado (obrigatório no Academy) | Habilitado |
| RDS Backup retention | 1 dia (mínimo) | 7 a 30 dias |
| RDS Enhanced Monitoring | Desabilitado (obrigatório) | Habilitado |
| Versão do Postgres | 15.19 (15.7 do plano original saiu de catálogo) | Última minor estável na data |
| NAT Gateway | 1 apenas (`single_nat_gateway = true`) | 1 por AZ |
| Versão do Kubernetes | 1.34 (1.30 do plano original perdeu suporte) | Última minor estável suportada |
| EKS Node group | 1 node `t3.small` | 3+ nodes, tipos maiores |
| PersistentVolume no Redis | `emptyDir` (sem addon `aws-ebs-csi-driver`, que exigiria `ec2:CreateVolume` fora do que a `LabEksNodeRole` permite) | PVC com StorageClass gerenciada |
| Módulo EKS Terraform | Recursos diretos do provider (`aws_eks_cluster`/`aws_eks_node_group`) | `terraform-aws-modules/eks/aws`, viável fora do Academy |
| IRSA para service accounts | Sem (Academy não permite OIDC provider) | Com |
| GitHub Actions OIDC | Sem (Academy não permite federação) | Com, sem long-lived keys |
| Secret da license key do New Relic | Variável de ambiente manual no `install-newrelic.sh` | Secrets Manager, como os demais secrets |
| Secret da API Key interna (Lambda ↔ API) | Ainda não provisionado (pendência real, ver `docs/arquitetura-fase3.md`) | Secrets Manager |

---

## Rigor arquitetural mantido apesar das limitações

- VPC com 2 AZs (RDS DB Subnet Group exige no mínimo 2 subnets em AZs diferentes)
- Subnets privadas para EKS nodes e RDS, nunca expostos direto à internet
- Security Groups restritivos com referência SG-to-SG entre EKS e RDS (`rds-ingress`), confirmado via `aws ec2 describe-security-group-rules` que a regra referencia o SG de verdade, não é só sintaxe
- NAT Gateway para tráfego outbound das subnets privadas (poderíamos ter colocado tudo em subnet pública para simplificar; optamos por não fazer)
- `storage_encrypted = true` no RDS
- Senhas e JWT secret key em Secrets Manager, nunca em texto plano
- Mascaramento de CPF em logs (ver ADR-008)
- Idempotency-Key middleware (`IdempotentAttribute` + `RedisIdempotencyStore`), com trade-offs conhecidos documentados no review do PR #44
- Healthchecks via middleware nativo do ASP.NET Core
- HPA configurado no Kubernetes
- CI/CD com branch protection, PR obrigatório, sem push direto em `main`

---

## Rotina de disciplina de custo

Ao final de cada sessão de testes:

```bash
# oficina-infra-k8s: apagar o Service LoadBalancer ANTES do destroy,
# senão o NLB fica órfão e trava o destroy da VPC depois
kubectl delete svc oficina-api -n oficina
cd terraform/envs/homolog && terraform destroy -auto-approve

# oficina-infra-db pode ficar de pé entre sessões de trabalho no mesmo dia
# (RDS + VPC ~US$1/dia), mas também deve ser destruído ao encerrar o ciclo de testes
```

Ao retomar: renovar credenciais no painel do Academy, reaplicar `oficina-infra-db` (se destruído) e depois `oficina-infra-k8s`, popular o secret K8s (`populate-secret.sh`) e aplicar os manifestos (`deploy-manifests.sh`).

---

## Alternativas descartadas

- **Conta AWS pessoal com cartão de crédito:** descartada, custo pessoal para os integrantes.
- **GCP/Azure:** descartada, sem créditos institucionais equivalentes para o grupo.
- **Tudo em compute local (Kind + docker-compose):** descumpre o requisito de deploy na nuvem da Fase 3. Foi a escolha da Fase 2 (ver ADR-003), superada por este projeto.

---

## Consequências

**Positivas:**
- Zero custo pessoal para os integrantes.
- Decisões não convencionais (recursos Terraform diretos em vez do módulo comunitário, `emptyDir` em vez de PVC) ficam documentadas e defensáveis, não escondidas.

**Negativas:**
- Alguns padrões de produção real ficam como pendência documentada (Multi-AZ, IRSA, OIDC, secrets do New Relic e da API interna).
- Risco de perder o orçamento da conta se a disciplina de destruir a infra não for mantida.

---

## Roadmap em produção real

1. **Curto prazo:** Multi-AZ, IRSA, GitHub Actions OIDC, secret da license key do New Relic e da API Key interna via Secrets Manager.
2. **Médio prazo:** read replicas no RDS, WAF na frente do API Gateway, backups estendidos.
3. **Longo prazo:** multi-região, auto scaling mais avançado, service mesh.

Esses itens não são erros do projeto atual, são o próximo passo natural depois de sair do Academy.

---

## Referências

- AWS Academy Learner Lab: https://awsacademy.instructure.com
- `oficina-infra-k8s/docs/ARCHITECTURE.md` — decisão detalhada sobre o módulo EKS
- `docs/arquitetura-fase3.md` (este repositório) — diagrama de componentes e pendências da Lambda
- [ADR-003](./ADR-003-desenho-infraestrutura-fase2.md) — desenho de infraestrutura da Fase 2, superado por este projeto na Fase 3
