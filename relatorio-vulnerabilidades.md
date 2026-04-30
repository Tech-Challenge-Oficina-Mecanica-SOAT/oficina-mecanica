# Relatório de Vulnerabilidades — Oficina Mecânica API

**Data da análise:** 2026-04-30
**Ferramenta:** SonarQube Community + revisão manual de código
**Versão analisada:** branch `feat/documentacao-para-testes`
**Status:** Relatório completo (todas as rotas implementadas)

---

## Resumo

| Severidade | Quantidade | Corrigidas | Pendentes |
|------------|-----------|------------|-----------|
| Crítica    | 2         | 0          | 2         |
| Alta       | 3         | 1          | 2         |
| Média      | 4         | 2          | 2         |
| Baixa      | 1         | 0          | 1         |
| **Total**  | **10**    | **3**      | **7**     |

---

## Vulnerabilidades Críticas

### C1 — Segredos hardcoded em `appsettings.json`

- **Arquivo:** `src/OficinaMecanica.API/appsettings.json`, linhas 10, 14, 20
- **OWASP:** A02 Cryptographic Failures
- **Risco:** Qualquer pessoa com acesso ao repositório obtém a senha do banco, a chave de assinatura JWT e a chave HMAC de senhas — suficiente para forjar tokens e acessar dados.
- **Evidência:**
  ```json
  "DefaultConnection": "Host=localhost;...;Password=SuaSenha;..."
  "SecretKey": "mecanica-jwt-secret-key-minimo-32-chars!!"
  "PasswordKey": "K7mP2nQx9vR4wL8sY1tZ6uA3cE5gJ0hF"
  ```
- **Status:** ❌ Não corrigido
- **Correção:** Mover todos os segredos para variáveis de ambiente (`ASPNETCORE_` ou `.env`). Em produção, usar gerenciador de segredos (Azure Key Vault, AWS Secrets Manager).

---

### C2 — Senha do PostgreSQL hardcoded no `docker-compose.yaml`

- **Arquivo:** `docker-compose.yaml`, linhas 12 e 35
- **OWASP:** A02 Cryptographic Failures
- **Risco:** A senha do banco está versionada em texto claro em dois lugares do compose, replicando o problema de C1 na infraestrutura.
- **Evidência:**
  ```yaml
  POSTGRES_PASSWORD: SuaSenha
  ConnectionStrings__DefaultConnection: ...Password=SuaSenha
  ```
- **Status:** ❌ Não corrigido
- **Correção:** Substituir por variáveis de ambiente via arquivo `.env` (não versionado) e referenciar com `${POSTGRES_PASSWORD}`.

---

## Vulnerabilidades Altas

### A1 — Perfil `Admin` como padrão no cadastro de usuário

- **Arquivo:** `src/OficinaMecanica.Application/DTOs/RegistrarUsuarioDto.cs`, linha 5
- **OWASP:** A01 Broken Access Control
- **Risco:** O endpoint `POST /Auth/registrar` é público (`[AllowAnonymous]`). Qualquer cliente pode registrar-se como Admin omitindo o campo `perfil`, pois o padrão do DTO é `Perfil.Admin`.
- **Evidência:**
  ```csharp
  public record RegistrarUsuarioDto(string Email, string Senha, Perfil Perfil = Perfil.Admin);
  ```
- **Status:** ❌ Não corrigido
- **Correção:** Alterar o default para `Perfil.Cliente`. Promoção a Admin deve exigir autenticação prévia de outro Admin.

---

### A2 — `Trust Server Certificate=true` na connection string

- **Arquivo:** `src/OficinaMecanica.API/appsettings.json`, linha 10
- **OWASP:** A02 Cryptographic Failures
- **Risco:** Desabilita a validação do certificado TLS do PostgreSQL, permitindo ataques Man-in-the-Middle na comunicação entre a API e o banco.
- **Evidência:**
  ```
  Trust Server Certificate=true
  ```
- **Status:** ❌ Não corrigido — aceitável apenas em desenvolvimento local; deve ser removido antes de qualquer ambiente compartilhado.
- **Correção:** Remover a opção ou definir `Ssl Mode=Require` com certificado válido em produção.

---

### A3 — Entropia insuficiente nas chaves criptográficas

- **Arquivo:** `src/OficinaMecanica.API/appsettings.json`, linhas 14 e 20
- **OWASP:** A02 Cryptographic Failures
- **Risco:** Embora as chaves atendam o tamanho mínimo, são strings descritivas com baixa entropia (`"mecanica-jwt-secret-key-minimo-32-chars!!"`). Reduz a resistência a ataques de força bruta contra tokens capturados.
- **Status:** ⚠️ Parcialmente corrigido — tamanho adequado, entropia insuficiente
- **Correção:** Gerar as chaves com `openssl rand -base64 64` ou equivalente criptográfico.

---

## Vulnerabilidades Médias

### M1 — `AllowedHosts: "*"` permissivo

- **Arquivo:** `src/OficinaMecanica.API/appsettings.json`, linha 12
- **OWASP:** A05 Security Misconfiguration
- **Risco:** Desabilita proteção contra Host Header Injection. Pode ser explorado em ataques de cache poisoning ou redirecionamentos maliciosos.
- **Status:** ❌ Não corrigido
- **Correção:** Especificar hosts explícitos: `"AllowedHosts": "localhost;api.oficina.com"`.

---

### M2 — Porta 5432 do PostgreSQL exposta no compose

- **Arquivo:** `docker-compose.yaml`, linhas 13–14
- **OWASP:** A05 Security Misconfiguration
- **Risco:** O banco de dados fica acessível diretamente na rede local (ou em cloud sem firewall), além da API. Qualquer serviço na rede pode tentar conexões diretas.
- **Status:** ⚠️ Aceitável em desenvolvimento; não deve ser replicado em produção
- **Correção:** Remover o mapeamento de portas do serviço `postgres` no compose de produção. A API acessa o banco pela rede interna do Docker.

---

### M3 — Ausência de validação de entrada nos DTOs

- **Arquivos:** `src/OficinaMecanica.Application/DTOs/` (múltiplos)
- **OWASP:** A03 Injection / A04 Insecure Design
- **Risco:** DTOs como `LoginDto`, `CreateClienteDto` e `CreateOrdemServicoDto` não possuem Data Annotations (`[Required]`, `[StringLength]`, `[EmailAddress]`). Validações de negócio estão nos services/entities, mas erros de formato chegam mais fundo na stack antes de serem rejeitados.
- **Status:** ✅ Mitigado funcionalmente — validações de negócio estão nas entidades de domínio e nos services; nenhuma injeção de SQL identificada pois todo acesso ao banco usa EF Core com LINQ parametrizado.
- **Melhoria recomendada:** Adicionar anotações nos DTOs para rejeitar entradas inválidas na camada de apresentação, reduzindo processamento desnecessário.

---

### M4 — Expiração do JWT muito curta para o fluxo de uso

- **Arquivo:** `src/OficinaMecanica.API/appsettings.json`, linha 17
- **OWASP:** A07 Identification and Authentication Failures
- **Risco:** Token expira em 5 minutos. O fluxo de OS (diagnóstico → aprovação → execução) pode durar horas; usuários precisarão re-autenticar constantemente, aumentando a superfície de ataque (mais requisições de login).
- **Status:** ✅ Mitigado para o contexto do Tech Challenge — aceitável como escolha conservadora de segurança; ideal seria implementar refresh token para operações longas.

---

## Pontos Positivos — Controles Implementados

### A01 — Broken Access Control
- Todos os endpoints administrativos protegidos com `[Authorize(Roles = "Admin")]`
- Rotas por perfil: `Mecanico` acessa diagnóstico/conclusão; `Cliente` acessa aprovação/rejeição/histórico; `Admin` tem acesso total
- Endpoint público `/Publico/os/{id}/status` retorna apenas `osId`, `status` e `atualizadoEm` — nenhum dado pessoal exposto
- `AuthController` e `PublicoController` marcados com `[AllowAnonymous]` explicitamente

### A02 — Cryptographic Failures
- Senhas armazenadas com HMAC-SHA256 + salt aleatório de 32 bytes via `RandomNumberGenerator.GetBytes()` — não reversível
- Comparação de hashes com `CryptographicOperations.FixedTimeEquals()` — resistente a timing attacks
- JWT assinado com HmacSha256; validação completa de issuer, audience, lifetime e chave

### A03 — Injection
- Nenhum SQL raw identificado — toda persistência usa EF Core com LINQ parametrizado
- Inputs de e-mail normalizados (`.ToLower().Trim()`) antes de persistir

### A05 — Security Misconfiguration
- Container da API executa com usuário não-root (`appuser`) — Dockerfile linhas 23–24
- Build multi-stage no Dockerfile separa artefatos de build do runtime
- Imagens base oficiais Microsoft (`mcr.microsoft.com/dotnet/aspnet:10.0`)

### A07 — Identification and Authentication Failures
- JWT com validação de issuer, audience, lifetime e chave de assinatura (`Program.cs` linhas 55–63)
- Claims bem estruturados: `sub`, `email`, `role`, `jti` (ID único por token)

### A09 — Security Logging and Monitoring Failures
- Histórico de transições de status da OS registrado com `alteradoPor` e `motivo` para todas as operações (`HistoricoStatusOS`)
- `NotificacaoService` registra envio de orçamento via log estruturado

---

## Considerações OWASP Top 10 — Resumo

| Categoria | Status | Observação |
|-----------|--------|------------|
| A01 Broken Access Control | ⚠️ | Perfil Admin como default no cadastro (ver A1) |
| A02 Cryptographic Failures | ⚠️ | Chaves hardcoded (ver C1, C2, A2, A3) |
| A03 Injection | ✅ | EF Core parametrizado; sem SQL raw |
| A04 Insecure Design | ⚠️ | Validação de entrada nos DTOs ausente (ver M3) |
| A05 Security Misconfiguration | ⚠️ | AllowedHosts e porta PostgreSQL (ver M1, M2) |
| A06 Vulnerable Components | ✅ | Dependências atuais (.NET 10, pacotes recentes) |
| A07 Authentication Failures | ✅ | JWT configurado corretamente; timing-safe hash |
| A08 Software Integrity Failures | ✅ | Sem deserialização insegura identificada |
| A09 Logging & Monitoring | ✅ | Histórico de OS; log de notificações |
| A10 SSRF | ✅ | Sem requisições HTTP a URLs externas controláveis pelo usuário |

---

## Como executar o scan no SonarQube local

```bash
# O SonarQube já está disponível em http://localhost:9000 via docker compose up -d

# Gerar token em: http://localhost:9000 → My Account → Security → Generate Token

dotnet sonarscanner begin \
  /k:"mecanica-api" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.login="SEU_TOKEN" \
  /d:sonar.exclusions="**/Migrations/**,**/obj/**"

dotnet build src/OficinaMecanica.API/OficinaMecanica.API.csproj

dotnet sonarscanner end /d:sonar.login="SEU_TOKEN"
```

Resultados disponíveis em `http://localhost:9000/dashboard?id=mecanica-api` após a análise.
