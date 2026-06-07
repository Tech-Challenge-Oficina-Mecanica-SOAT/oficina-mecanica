# ADR-002 — Refatoração para Clean Architecture Estrita

| Campo        | Valor                          |
|--------------|-------------------------------|
| **Status**   | Aceito                         |
| **Data**     | 2026-06-06                     |
| **Autores**  | Vinicius             |
| **Contexto** | Fase 2 — Tech Challenge FIAP   |

---

## Contexto

Na Fase 1 a solução já separava os projetos em quatro camadas (Domain, Application, Infrastructure, API), mas violava as regras de independência da Clean Architecture em vários pontos:

- `Application` referenciava `Microsoft.Extensions.Configuration`, `System.IdentityModel.Tokens.Jwt` e `Konscious.Security.Cryptography` diretamente, acoplando a camada de negócio a frameworks externos
- `OrdemServicoService` acumulava múltiplos casos de uso em um único serviço (violação do SRP)
- Exceptions eram usadas como controle de fluxo entre camadas
- DTOs de entrada e saída misturados na mesma pasta
- Comunicação entre use cases via chamada direta em vez de eventos de domínio

---

## Decisões

### 1. Abstrações de infraestrutura na Application

**Decisão:** criar interfaces em `Application/Interfaces` para cada detalhe de infraestrutura:

| Interface | Implementação | Pacote removido de Application |
|---|---|---|
| `ITokenGenerator` | `Infrastructure/Auth/JwtTokenGenerator` | `System.IdentityModel.Tokens.Jwt` |
| `IPasswordHasher` | `Infrastructure/Security/Argon2PasswordHasher` | `Konscious.Security.Cryptography` |
| `IAppLogger<T>` | `Infrastructure/Logging/AppLogger<T>` | `Microsoft.Extensions.Logging` |
| `IJwtSettings` | `API/Configuration/JwtSettings` | `Microsoft.Extensions.Configuration` |

A `API` injeta as implementações concretas via DI. A `Application` só conhece as interfaces.

**Por quê:** a regra de dependência da Clean Architecture exige que `Application` não conheça detalhes de framework. Qualquer referência direta cria um acoplamento que impede trocar o framework sem tocar na lógica de negócio.

---

### 2. Um Use Case por operação

**Decisão:** quebrar `OrdemServicoService` (e demais serviços) em classes individuais, cada uma implementando uma única interface com um único método `ExecutarAsync`.

Estrutura resultante:
```
Application/UseCases/
├── OrdemServico/
│   ├── AbrirOrdemServico/
│   ├── AdicionarItensOS/
│   ├── ConsultarOrdemServico/
│   ├── ListarOrdensServico/
│   ├── ObterTempoMedioExecucao/
│   └── RemoverItemOS/
└── OrdemServicoStatus/
    ├── AprovarOS/
    ├── AprovarOrcamentoPorEmail/
    ├── EntregarOS/
    ├── ForcarStatusOS/
    ├── IniciarDiagnostico/
    ├── MarcarAguardandoAprovacao/
    ├── NotificarConclusao/
    ├── ObterHistoricoOS/
    └── RejeitarOS/
```

**Por quê:** serviços que acumulam casos de uso têm coesão baixa, dificultam testes unitários (cada test class precisa mockar dependências de todos os métodos) e violam o princípio de responsabilidade única.

---

### 3. Result\<T> em vez de exceções como fluxo de controle

**Decisão:** todos os use cases retornam `Result<T>` (definido em `Application/Common/Result.cs`). Exceções só são lançadas para erros verdadeiramente excepcionais (falha de infra, invariante de domínio).

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }   // None | Validation | NotFound | Conflict | Unauthorized

    public static Result<T> Success(T value)         => ...;
    public static Result<T> NotFound(string error)   => ...;
    public static Result<T> Validation(string error) => ...;
    public static Result<T> Conflict(string error)   => ...;
}
```

Os controllers traduzem `Result.ErrorType` em HTTP status codes via `MapError()`.

**Por quê:** É estabelecido que use cases não devem lançar exceções de lógica de negócio. Exceções são caras, dificultam o rastreamento do fluxo e acoplam implicitamente chamador e chamado ao tipo da exceção.

---

### 4. Value Objects no Domínio

**Decisão:** conceitos com regras de validação próprias são modelados como Value Objects imutáveis em `Domain/ValueObjects`:

| VO | Regra principal |
|---|---|
| `Email` | Validação via `MailAddress.TryCreate` (RFC-compatível); normalizado para lowercase |
| `Documento` | CPF (11 dígitos + dígitos verificadores) ou CNPJ (14 alfanuméricos, inclui formato Mercosul 2026) |
| `Telefone` | 10–13 dígitos após limpeza |
| `Placa` | Formato antigo `AAA9999` ou Mercosul `AAA9A99` após normalização uppercase |

Os VOs são persistidos como strings via `ValueConverter` no EF Core; o banco armazena o valor primitivo e o ORM hidrata o VO na leitura.

**Por quê:** strings sem invariantes criam um modelo anêmico onde a validação fica dispersa em serviços e controllers. VOs encapsulam a regra de negócio no lugar correto (Domínio) e eliminam estados inválidos em tempo de compilação.

---

### 5. Domain Events com dispatch centralizado no SaveChangesAsync

**Decisão:** entidades herdam de `Entity` (base class) que expõe `RaiseEvent` / `DomainEvents` / `ClearEvents`. O `ApplicationDbContext.SaveChangesAsync` coleta os eventos antes de persistir, salva e depois despacha via `IDomainEventDispatcher`.

```csharp
// ApplicationDbContext.SaveChangesAsync
var entidades = ChangeTracker.Entries<Entity>()
    .Where(e => e.Entity.DomainEvents.Any())
    .Select(e => e.Entity).ToList();

var eventos = entidades.SelectMany(e => e.DomainEvents).ToList();
var resultado = await base.SaveChangesAsync(cancellationToken);

if (_dispatcher is not null && eventos.Count > 0)
{
    await _dispatcher.DispatchAsync(eventos);
    foreach (var entity in entidades) entity.ClearEvents();
}
```

Eventos de domínio implementados: `OrcamentoEnviadoEvent`, `OrdemAprovadaEvent`, `OrdemRejeitadaEvent`, `OrdemConcluidaEvent`, `OrdemEntregueEvent`.

**Por quê:** despacho manual em cada use case é propenso a esquecimentos e duplicações. A centralização no `SaveChangesAsync` garante que eventos são sempre disparados após a persistência bem-sucedida, sem que o programador precise lembrar de fazê-lo.

---

### 6. DTOs organizados por direção

**Decisão:** pasta `Application/DTOs` dividida em `Requests/` (entrada do use case) e `Responses/` (saída).

**Por quê:** misturar DTOs de entrada e saída na mesma pasta viola a separação conceitual descrita no capítulo 22 da cartilha (*Request Model* vs *Response Model*) e dificulta encontrar a estrutura correta ao escrever use cases ou controllers.

---

## Consequências

**Positivas:**
- `Application.csproj` não referencia nenhum pacote de framework (EF Core, JWT, Argon2, Logging) — compilável sem infraestrutura
- Cada use case é testável com mocks simples: uma interface de repositório + um `Result<T>` esperado
- Domain Events permitem adicionar comportamentos (e-mail, auditoria, métricas) sem modificar use cases existentes
- Value Objects eliminam estados inválidos no Domínio; erros de validação são detectados na borda do sistema (controller → use case)

**Trade-offs:**
- Mais arquivos: 49 use cases vs. ~6 serviços anteriores — navegar pelo projeto requer IDE com bom suporte a "go to definition"
- `Result<T>` requer que todos os controllers implementem o mapeamento `ErrorType → HTTP status`; controladores que esquecem o `MapError` retornarão 200 em caso de erro

---

## Arquivos relevantes

| Caminho | Responsabilidade |
|---|---|
| `Application/Common/Result.cs` | Result pattern com ErrorType |
| `Application/Interfaces/ITokenGenerator.cs` | Abstração de geração de JWT |
| `Application/Interfaces/IPasswordHasher.cs` | Abstração de hash de senha |
| `Application/Interfaces/IAppLogger.cs` | Abstração de logging |
| `Application/Configuration/IJwtSettings.cs` | Abstração de config JWT |
| `Application/UseCases/` | 49 use cases organizados por agregado |
| `Application/DTOs/Requests/` | Request Models |
| `Application/DTOs/Responses/` | Response Models |
| `Application/Mappers/OrdemServicoMapper.cs` | Mapeamento de entidade para DTO |
| `Domain/ValueObjects/` | Email, Documento, Telefone, Placa |
| `Domain/Events/` | OrcamentoEnviadoEvent, OrdemAprovadaEvent, etc. |
| `Infrastructure/Auth/JwtTokenGenerator.cs` | Implementação JWT |
| `Infrastructure/Security/Argon2PasswordHasher.cs` | Implementação Argon2id |
| `Infrastructure/Logging/AppLogger.cs` | Wrapper sobre ILogger\<T\> |
| `Infrastructure/Data/ApplicationDbContext.cs` | Dispatch centralizado de Domain Events |
