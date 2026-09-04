# ADR-008 — Tratamento de Dados Pessoais (LGPD)

| Campo        | Valor                          |
|--------------|-------------------------------|
| **Status**   | Aceito, parcialmente implementado |
| **Data**     | 2026-08-27                     |
| **Autores**  | grupo Tech Challenge Oficina Mecânica |
| **Contexto** | Fase 3 — Tech Challenge FIAP   |

---

## Contexto

A aplicação processa dados pessoais de clientes: CPF, email, telefone, endereço. A LGPD (Lei 13.709/2018) exige tratamento adequado desses dados, e a Fase 5 do Tech Challenge terá disciplina específica sobre LGPD/GDPR. Esta ADR antecipa controles básicos de proteção de dados desde a Fase 3, documentando tanto o que já está implementado quanto o que ainda falta.

---

## Decisão

Aplicar os seguintes controles, alguns já em código e outros como próximo passo:

### 1. Encryption at rest — ✅ implementado

`storage_encrypted = true` no `aws_db_instance` do RDS (`oficina-infra-db/modules/rds/main.tf`), com a KMS key gerenciada pela própria AWS (não criamos KMS customizada, o Academy limita a criação de recursos IAM associados).

### 2. Mascaramento de CPF em logs — ✅ implementado

Nunca logar CPF completo. Implementado via `CpfMaskingTextFormatter` (Serilog), registrado em `Program.cs` da API .NET. Formato mascarado: `123.***.***-00`.

A Lambda `auth-cpf` (Node.js), quando existir, precisa replicar essa mesma regra antes de logar qualquer request; hoje não há código lá para aplicar isso (`oficina-lambda-auth` está vazio).

### 3. Registro de consentimento — ⏳ não implementado

Ainda não existem os campos `DataConsentimentoLgpd` (nullable DateTime) e `IpConsentimento` (nullable string) na entidade `Cliente`, nem o método `RegistrarConsentimentoLgpd(string ip)`. Fica como próximo passo, coordenado com quem mantém o schema de `Clientes`.

Ao implementar: clientes existentes deverão receber `DataConsentimentoLgpd = NOW()` (grandfathering) ou `NULL` (solicitado no próximo acesso), a decidir junto com o time.

### 4. Comunicação criptografada — parcial

- API Gateway (quando existir): HTTPS obrigatório por padrão do serviço.
- RDS: SSL não está forçado hoje via parameter group. O AWS Academy pode ter limitações aqui; precisa validação antes de forçar.
- Lambda → API interna (`/internal/auth/cpf-verify`): quando a Lambda existir, a chamada deve ser HTTPS (o NLB hoje expõe HTTP simples; avaliar TLS termination antes de considerar isso resolvido).

### 5. Retenção mínima — ✅ implementado, dentro do possível no Academy

- Backup do RDS: 1 dia (`backup_retention_period = 1`), o mínimo permitido. Em produção real: 7 a 30 dias.
- Logs no New Relic: janela padrão do tier gratuito (cerca de 7 dias).

### 6. Direito ao esquecimento — ⏳ não implementado

Não faz parte do escopo da Fase 3, será tratado na Fase 5. A separação de dados pessoais na entidade `Cliente` (isolada de `OrdemServico`, `Veiculo`) já facilita um futuro `DELETE` em cascata controlado. IDs não expõem PII: são Guids aleatórios, não derivados de CPF.

---

## Base legal aplicada

LGPD Art. 7º, execução de contrato entre titular (cliente) e controlador (oficina). Não é necessário consentimento explícito para o tratamento operacional, mas o registro de consentimento (item 3, pendente) segue sendo recomendado por transparência.

---

## Consequências

**Positivas:**
- Encryption at rest, mascaramento de CPF e retenção mínima já estão em produção, não em backlog.
- A Fase 5 encontra uma base parcial pronta, reduzindo o retrabalho.

**Negativas:**
- Registro de consentimento e forçar TLS no RDS ficam como dívida conhecida, não implícita.
- A Lambda `auth-cpf`, quando construída, precisa reaplicar o mascaramento de CPF em Node.js; é fácil esquecer isso numa linguagem diferente da API.

---

## Referências

- LGPD (Lei 13.709/2018): https://www.gov.br/anpd/pt-br
- AWS Compliance Programs: https://aws.amazon.com/compliance/programs/
- `src/OficinaMecanica.Infrastructure/Logging/CpfMaskingTextFormatter.cs`
- `oficina-infra-db/modules/rds/main.tf`
