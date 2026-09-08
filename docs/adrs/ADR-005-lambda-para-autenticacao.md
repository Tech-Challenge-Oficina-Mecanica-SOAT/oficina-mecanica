# ADR-005 — Lambda para Autenticação por CPF

| Campo        | Valor                          |
|--------------|-------------------------------|
| **Status**   | Aceito, implementado e testado |
| **Data**     | 2026-09-08 (retroativa; decisão tomada e implementada em `oficina-lambda-auth` antes desta data, formalizada agora) |
| **Autores**  | grupo Tech Challenge Oficina Mecânica |
| **Contexto** | Fase 3 — Tech Challenge FIAP   |

---

## Contexto

A Fase 3 exige uma função serverless que valide o CPF do cliente, consulte o banco de dados e devolva um JWT, isolada da aplicação principal.

---

## Decisão

Implementar em **AWS Lambda com Node.js 20**, no repositório `oficina-lambda-auth`.

## Razão

- Cold start baixo comparado a runtimes com máquina virtual/JIT mais pesado (.NET, Java).
- Bibliotecas de validação de CPF e assinatura de JWT (`jsonwebtoken`) são leves em Node, sem SDK adicional além do `pg` e dos clients da AWS.
- Pipeline de build simples: `npm ci` + zip, sem passo de compilação nativa.
- Isolamento de falhas: um erro na Lambda não derruba a API principal, e vice-versa.
- Dentro do tier gratuito da AWS para o volume esperado no projeto.

## Alternativas descartadas

- **.NET em Lambda**: manteria a mesma stack da API principal, mas cold start pior e complexidade de build adicional sem benefício real aqui.
- **Endpoint dentro da própria API .NET**: descumpre o requisito explícito do enunciado ("criar uma Function Serverless" que valida, consulta e gera o token).
- **Container Lambda**: overhead desnecessário para uma função com lógica tão simples.

---

## Decisão que estava pendente no planejamento original, agora resolvida: como a Lambda acessa o cliente

Duas opções foram cogitadas:
1. **Lambda consulta o RDS diretamente** (driver `pg`).
2. **Lambda chama um endpoint interno da API .NET** (`POST /internal/auth/cpf-verify`, protegido por API Key própria) que faz a consulta e emite o token.

**Escolhida a Opção 1.** O endpoint interno chegou a ser implementado em `oficina-mecanica` (`InternalAuthController`/`AutenticarPorCpfUseCase`) antes dessa decisão ser fechada, e permanece no código, mas não é o caminho usado pela Lambda. A razão decisiva: o enunciado da Fase 3 pede literalmente que a própria Function Serverless **valide o CPF**, **consulte o banco** e **gere o token** — delegar a consulta e a emissão do token ao endpoint interno faria a Lambda deixar de cumprir duas das três responsabilidades exigidas, com risco real de perda de nota num requisito específico e literal do desafio.

---

## Implementação real

- `src/auth/cpfValidator.js`: valida os dígitos verificadores do CPF (com ou sem máscara).
- `src/auth/clienteRepository.js`: consulta `SELECT "Id", "Email", "Ativo" FROM "Clientes" WHERE "Documento" = $1` via `pg`, dentro da VPC (subnets privadas, mesmo Security Group do cluster EKS liberado no RDS).
- `src/auth/jwtGenerator.js`: assina um JWT HS256 com a mesma `jwt-secret-key` do Secrets Manager usada pela API principal, expirando em 1 hora. Claims: `sub` (ID do cliente), `email`, `role: "Cliente"`, `iss: "oficina-mecanica-lambda"`, `aud: "mecanica-cliente"`.
- `src/config/secrets.js`: busca endpoint/porta/nome/usuário do RDS no Parameter Store e a senha no Secrets Manager, com cache em memória (mantido entre invocações warm da Lambda).
- Função configurada com timeout de 30s e 256 MB de memória, dentro de VPC (`vpc_config` apontando para as subnets privadas publicadas por `oficina-infra-db`).

## Bugs reais encontrados e corrigidos testando de ponta a ponta

1. **Case das colunas retornadas pela query**: a query usa identificadores entre aspas (`"Id"`, `"Email"`, `"Ativo"`), e o Postgres preserva o case exato — as colunas reais são PascalCase (confirmado contra a migration do EF Core). O código acessava `cliente.id`/`cliente.email`/`cliente.ativo` (minúsculo, sempre `undefined`), fazendo **todo cliente ativo cair em 403 "Cliente inativo"**. Corrigido para acessar as chaves no case correto.
2. Ver [ADR-004](./ADR-004-api-gateway-pattern.md) para os bugs de integração com o EKS (não específicos desta Lambda, mas descobertos no mesmo ciclo de testes).

## Evidência de teste real

Confirmado via chamadas HTTP reais contra a infraestrutura em `homolog`:

| Cenário | Resultado |
|---|---|
| CPF válido, cliente ativo | `200` + JWT válido |
| CPF válido, cliente inativo | `403` "Cliente inativo" |
| CPF válido, não cadastrado | `404` "Cliente não encontrado" |
| CPF com formato inválido | `400` "CPF inválido" |
| CPF ausente no corpo | `400` "CPF é obrigatório" |
| JWT emitido usado contra a API .NET (endpoint restrito a Admin) | `403`, prova que o token atravessa toda a cadeia e é validado corretamente |

---

## Consequências

**Positivas:**
- As três responsabilidades exigidas pelo enunciado (validar, consultar, emitir) ficam realmente na Function Serverless, não distribuídas.
- Testado de ponta a ponta pela primeira vez nesta fase, com todos os cenários de erro documentados acima confirmados.

**Negativas:**
- A Lambda duplica a lógica de acesso ao RDS que a API principal também tem (acoplamento a mais um schema de banco, em vez de centralizar em um único ponto de acesso a dados). Aceito conscientemente pela razão descrita acima.
- `InternalAuthController`/`AutenticarPorCpfUseCase` (em `oficina-mecanica`) ficam como código não utilizado no caminho principal de autenticação; não removidos porque não é impreciso que o endpoint exista, só deixou de ser o caminho recomendado.

---

## Referências

- `oficina-lambda-auth/src/`
- `oficina-lambda-auth/terraform/modules/lambda/main.tf`
- [ADR-004](./ADR-004-api-gateway-pattern.md) — API Gateway que expõe esta Lambda
- [ADR-007](./ADR-007-segregacao-repositorios.md) — contratos entre este repositório e os demais
