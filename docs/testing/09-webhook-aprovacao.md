# 09. Webhook de Aprovação de Orçamento

Fluxo de aprovação/rejeição de orçamento via link enviado por e-mail (webhook externo).

## Funcionamento

1. Quando a OS entra em `AguardandoAprovacao`, um e-mail é enviado ao cliente
2. O e-mail contém dois links: **APROVAR** e **RECUSAR**
3. O cliente clica em um dos links
4. O sistema processa a escolha e atualiza o status da OS
5. Uma página de confirmação é exibida para o cliente

## Endpoint (público - não requer token JWT)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/webhooks/ordens-servico/aprovar/{token}?aprovado=true/false` | Aprova ou recusa o orçamento |

**Parâmetros:**
- `token` (path): Token único gerado para a OS
- `aprovado` (query): `true` para aprovar, `false` para recusar

## Pré-condições

- OS existente no status `AguardandoAprovacao`
- Token válido e não utilizado

---

## Caminho feliz

### 1. Aprovar orçamento

Acesse o link recebido por e-mail ou simule:

GET http://localhost:5000/api/webhooks/ordens-servico/aprovar/{token}?aprovado=true

**Resposta (HTML):**
✅ Orçamento Aprovado!
Sua ordem de serviço entrou em execução.

**Resultado:**
- Status da OS: `AguardandoAprovacao → EmExecucao`
- Token marcado como usado

---

### 2. Recusar orçamento

GET http://localhost:5000/api/webhooks/ordens-servico/aprovar/{token}?aprovado=false

**Resposta (HTML):**
❌ Orçamento Recusado
Sua ordem de serviço foi cancelada.
Entre em contato para negociar um novo orçamento.

**Resultado:**
- Status da OS: `AguardandoAprovacao → Rejeitada`
- Token marcado como usado

---

## Caminho triste

| Cenário | Resposta esperada |
|---------|-------------------|
| Token inválido | Página de erro: "Token inválido" |
| Token já utilizado | Página de erro: "Link já foi utilizado" |
| OS não está em `AguardandoAprovacao` | Página de erro com status atual da OS |

---

## Fluxo completo de teste

1. Criar uma OS via API (Admin)
2. Adicionar itens (serviços/peças)
3. Enviar para aprovação (Admin) → OS fica `AguardandoAprovacao`
4. Verificar no MailHog (`http://localhost:8025`) o e-mail com os links
5. Clicar no link de aprovação (ou copiar o token e montar a URL)
6. Confirmar que a OS mudou para `EmExecucao`
7. Tentar usar o mesmo token novamente → deve falhar

---

## E-mail enviado

O e-mail de orçamento contém:

- Número da OS
- Valor total
- Tabela com itens (descrição, quantidade, valor unitário, subtotal)
- Botões/link para **APROVAR** e **RECUSAR**

> O link é de uso único e cada nova aprovação gera um novo token.

---

## Próximo passo

Voltar para [README principal](../../README.md)