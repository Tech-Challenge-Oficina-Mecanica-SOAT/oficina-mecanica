# Relatório de Vulnerabilidades — Oficina Mecânica API

**Data da análise:** 2026-05-03  
**Ferramenta:** SonarQube Community Build 26.4.0.121862  
**Versão analisada:** branch `main`  
**Quality Gate:** ✅ **Passed**  
**Dashboard:** `http://localhost:9000/dashboard?id=mecanica-api`

---

## Resumo — Dashboard SonarQube

| Dimensão | Issues | Rating |
|---|---|---|
| Security (Vulnerabilidades) | 2 | C |
| Reliability | 2 | C |
| Maintainability | 19 | A |
| Security Hotspots | 4 | E |
| Coverage | **90,5%** | — |
| Duplications | 0,0% | — |

### Corrigidos desde a análise anterior (2026-05-01)

| Regra | Arquivo | Descrição |
|---|---|---|
| `csharpsquid:S1135` | `OrdemServicoStatusController.cs:88` | Comentário TODO pendente — removido |
| `external_roslyn:CS8604` | `OrdemServicoStatusControllerTests.cs:146` | Possível null em `Enumerable.Last()` — null-forgiving adicionado |
| `external_roslyn:CS8618` | `Veiculo.cs:16` | Propriedade `Placa` não anulável sem inicialização — corrigido |
| `csharpsquid:S1905` | `ServicosController.cs:66` | Cast desnecessário para `IEnumerable<ServicoDto>` — removido |

---

## Vulnerabilidades (type = VULNERABILITY)

### V1 — Credencial hardcoded na connection string

- **Regra:** `csharpsquid:S2068`
- **Arquivo:** `src/OficinaMecanica.API/appsettings.json`, linha 10
- **Severidade:** MAJOR | **Impacto:** SECURITY (MEDIUM)
- **Mensagem:** *"password" detected here, make sure this is not a hard-coded credential.*
- **OWASP:** A02 Cryptographic Failures
- **Risco:** A senha do PostgreSQL está em texto claro no arquivo versionado. Qualquer acesso ao repositório expõe o banco de dados.
- **Correção:** Mover para variável de ambiente (`ASPNETCORE_ConnectionStrings__DefaultConnection`) e remover o valor do `appsettings.json`.

---

### V2 — Chave criptográfica hardcoded (PasswordKey)

- **Regra:** `csharpsquid:S2068`
- **Arquivo:** `src/OficinaMecanica.API/appsettings.json`, linha 20
- **Severidade:** MAJOR | **Impacto:** SECURITY (MEDIUM)
- **Mensagem:** *"password" detected here, make sure this is not a hard-coded credential.*
- **OWASP:** A02 Cryptographic Failures
- **Risco:** A chave HMAC usada para hash de senhas está versionada. Qualquer pessoa com acesso ao repositório pode replicar o mecanismo de hash.
- **Correção:** Mover para variável de ambiente. Gerar chave com entropia suficiente: `openssl rand -base64 64`.

---

## Security Hotspots (4 identificados)

Security Hotspots são pontos que **requerem revisão manual** — o SonarQube os sinaliza como potencialmente sensíveis, mas não classifica automaticamente como vulnerabilidade. Os 4 hotspots identificados estão disponíveis para triagem em:

`http://localhost:9000/security_hotspots?id=mecanica-api`

Cada hotspot deve ser avaliado e marcado como **"Fixed"** (corrigido) ou **"Safe"** (falso positivo justificado) após revisão.

---

## Issues de Confiabilidade (Reliability — Rating C)

### R1 — Chamada síncrona a `Migrate()` em contexto assíncrono

- **Regra:** `csharpsquid:S6966`
- **Arquivo:** `src/OficinaMecanica.API/Program.cs`, linha 101
- **Severidade:** MAJOR | **Impacto:** RELIABILITY (MEDIUM)
- **Mensagem:** *Await MigrateAsync instead.*
- **Risco:** Bloqueia a thread durante a migration no startup, podendo causar deadlock em ambientes com SynchronizationContext.
- **Correção:** Substituir `context.Database.Migrate()` por `await context.Database.MigrateAsync()`.

---

### R2 — Chamada síncrona a `Run()` em contexto assíncrono

- **Regra:** `csharpsquid:S6966`
- **Arquivo:** `src/OficinaMecanica.API/Program.cs`, linha 104
- **Severidade:** MAJOR | **Impacto:** RELIABILITY (MEDIUM)
- **Mensagem:** *Await RunAsync instead.*
- **Correção:** Substituir `app.Run()` por `await app.RunAsync()`.

---

## Issues de Manutenibilidade — principais (Maintainability — Rating A)

### Código-fonte (MAIN)

| Regra | Arquivo | Linha | Severidade | Descrição |
|---|---|---|---|---|
| `S1118` | `Program.cs` | 106 | MAJOR | Classe `Program` sem constructor `protected` ou modificador `static` |
| `S1144` | `Veiculo.cs` | 14 | MAJOR | Setter privado `set_Cliente` não utilizado — remover |
| `S3267` ⚠️ *novo* | `OrdemServicoService.cs` | 70 | MINOR | Loop pode ser simplificado usando o método LINQ `Where` |
| `S1192` | `ServicosController.cs` | 117 | MINOR | Literal `"Serviço não encontrado"` repetida 4 vezes — extrair constante |
| `S1192` | `ExampleSchemaTransformer.cs` | 14 | MINOR | Literal `"email"` repetida 4 vezes — extrair constante |
| `S1192` | `ExampleSchemaTransformer.cs` | 54 | MINOR | Literal `"descricao"` repetida 4 vezes — extrair constante |
| `CA1873` | `NotificacaoService.cs` | 14, 23 | INFO | Interpolação de string avaliada mesmo quando logging está desabilitado — usar `LogInformation(template, args)` |
| `CA1860` | `OrdemServicoRepository.cs` | 79 | INFO | Preferir `.Count > 0` a `.Any()` por performance |
| `ASP0025` | `Program.cs` | 70 | INFO | Usar `AddAuthorizationBuilder` em vez de `AddAuthorization` com callback |
| `ASP0027` | `Program.cs` | 106 | INFO | `public partial class Program` não é mais necessário no .NET 10 |

### Testes (TEST)

| Regra | Arquivo | Linha | Severidade | Descrição |
|---|---|---|---|---|
| `S2699` | `NotificacaoServiceTests.cs` | 18 | **BLOCKER** | Teste sem nenhuma assertion — não valida comportamento |
| `CA1822` ⚠️ *novo* | `OrdemServicosControllerTests.cs` | 23 | INFO | Método `SeedOSAsync` não acessa dados de instância — pode ser `static` |
| `CA1822` | `OrdemServicoStatusServiceTests.cs` | 26 | INFO | Método `OSRecebida` pode ser `static` |
| `CA1806` | `HistoricoStatusOSTests.cs` | 32, 41, 50 | INFO | Instâncias criadas mas nunca usadas nos testes de exceção |
| `CA1806` | `OrdemServicoTests.cs` | 33, 40 | INFO | Instâncias criadas mas nunca usadas nos testes de exceção |

---

## Análise Complementar — OWASP Top 10 (revisão manual)

O SonarQube detecta vulnerabilidades de código mas não cobre falhas de design de negócio. Os itens abaixo complementam a análise automatizada:

| Categoria OWASP | Status | Achado |
|---|---|---|
| **A01 Broken Access Control** | ⚠️ | `RegistrarUsuarioDto` tem `Perfil.Admin` como valor padrão — endpoint público permite auto-promoção a Admin |
| **A02 Cryptographic Failures** | ⚠️ | Detectado pelo SonarQube (V1 e V2 acima). Adicionalmente: `Trust Server Certificate=true` desabilita validação TLS do PostgreSQL |
| **A03 Injection** | ✅ | Sem SQL raw — todo acesso usa EF Core com LINQ parametrizado |
| **A04 Insecure Design** | ⚠️ | DTOs sem Data Annotations (`[Required]`, `[EmailAddress]`); validação ocorre só nas entidades de domínio |
| **A05 Security Misconfiguration** | ⚠️ | `AllowedHosts: "*"` em `appsettings.json`; porta 5432 do PostgreSQL exposta no `docker-compose.yaml` |
| **A06 Vulnerable Components** | ✅ | Dependências atuais (.NET 10, pacotes recentes sem CVEs conhecidos) |
| **A07 Authentication Failures** | ✅ | JWT com validação de issuer, audience, lifetime e chave; hash de senhas com HMAC-SHA256 + salt via `RandomNumberGenerator`; comparação timing-safe com `CryptographicOperations.FixedTimeEquals()` |
| **A08 Software Integrity** | ✅ | Sem deserialização insegura identificada |
| **A09 Logging & Monitoring** | ✅ | Histórico completo de transições de OS com `alteradoPor` e `motivo`; log estruturado no `NotificacaoService` |
| **A10 SSRF** | ✅ | Sem requisições HTTP a URLs controladas pelo usuário |

---

## Controles de Segurança Implementados

- **Autenticação JWT** completa: validação de issuer, audience, lifetime e chave de assinatura (`Program.cs` linhas 55–63)
- **Autorização por perfil** em todos os controllers: `[Authorize(Roles = "Admin")]`, `[Authorize(Roles = "Admin,Mecanico")]`, etc.
- **Hash de senhas** com HMAC-SHA256 + salt aleatório de 32 bytes — não reversível
- **Comparação timing-safe** de hashes com `CryptographicOperations.FixedTimeEquals()`
- **Sem SQL raw** — toda persistência via EF Core com LINQ parametrizado
- **Container não-root**: Dockerfile cria e usa `appuser` com `groupadd`/`useradd`
- **Build multi-stage** no Dockerfile: artefatos de build não acompanham a imagem de runtime
- **Endpoint público** (`/Publico/os/{id}/status`) retorna apenas `osId`, `status` e `atualizadoEm` — sem dados pessoais

---

## Como reproduzir a análise

```powershell
# 1. Subir o SonarQube (já incluso no docker-compose)
docker compose up -d sonarqube

# 2. Acessar http://localhost:9000, criar projeto "mecanica-api" e gerar token

# 3. Executar o scan (PowerShell ou CMD — não Git Bash)
dotnet sonarscanner begin /k:"mecanica-api" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="SEU_TOKEN" /d:sonar.exclusions="**/Migrations/**,**/obj/**"
dotnet build OficinaMecanica.slnx
dotnet sonarscanner end /d:sonar.token="SEU_TOKEN"
```

Resultados em: `http://localhost:9000/dashboard?id=mecanica-api`

### Exportar issues para CSV

```powershell
# Requer Docker. Gera sonarqube_issues.csv com todos os issues abertos.
powershell.exe -ExecutionPolicy Bypass -File "run-export.ps1" -Token "SEU_TOKEN" -Format "csv"
```

O script está disponível em `J:\Dev\sonarqube-issues-export-to-excel\`.
