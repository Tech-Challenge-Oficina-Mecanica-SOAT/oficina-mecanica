# Relatório de Vulnerabilidades — Mecanica API

## Data do scan: [a preencher após rodar o SonarQube]
## Ferramenta: SonarQube
## Status: Rascunho inicial (preencher após scan)

---

## Resumo

| Severidade | Quantidade | Corrigidas |
|------------|-----------|------------|
| Crítica    | -         | -          |
| Alta       | -         | -          |
| Média      | -         | -          |
| Baixa      | -         | -          |

---

## Vulnerabilidades críticas e altas

> Preencher após execução do scan SonarQube.

---

## Considerações OWASP Top 10 aplicadas

### A01 — Broken Access Control
- Todos os endpoints administrativos protegidos com `[Authorize(Roles = "Admin")]`
- Endpoint público `/publico/os/{id}/status` expõe apenas `osId`, `status` e `atualizadoEm` — nenhum dado pessoal do cliente
- `AuthController` e `PublicoController` marcados com `[AllowAnonymous]` explicitamente

### A02 — Cryptographic Failures
- Senhas armazenadas com HMAC-SHA256 + salt aleatório de 32 bytes (não reversível)
- Chave de assinatura JWT (`SecretKey`) separada da chave de hash de senhas (`PasswordKey`)
- Em produção, ambas as chaves devem vir de variáveis de ambiente (não do `appsettings.json`)
- Token JWT com expiração de 5 minutos

### A03 — Injection
- Uso de EF Core com LINQ parametrizado — sem SQL raw exposto na camada M4
- Entradas de email normalizadas (`.ToLower().Trim()`) antes de persistir

### A07 — Identification and Authentication Failures
- JWT com validação de issuer, audience, lifetime e chave de assinatura
- Comparação de hash via `CryptographicOperations.FixedTimeEquals` (timing-safe)
- Tokens de curta duração (5 min) reduzem janela de comprometimento

---

## Instruções para executar o scan

```bash
# Instalar scanner (uma vez)
dotnet tool install --global dotnet-sonarscanner

# Iniciar análise
dotnet sonarscanner begin /k:"mecanica-api" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="TOKEN"

# Build
dotnet build

# Finalizar e enviar resultados
dotnet sonarscanner end /d:sonar.login="TOKEN"
```

---

> **Nota:** Este relatório será atualizado em `feat/m4-final-report` após o scan completo com código integrado de M1, M2 e M3.
