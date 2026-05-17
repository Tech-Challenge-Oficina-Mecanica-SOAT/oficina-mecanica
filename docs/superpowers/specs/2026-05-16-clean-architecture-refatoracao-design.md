# Design: Refatoração para Clean Architecture

**Data:** 2026-05-16
**Branch:** feat/clean-architecture
**Contexto:** Tech Challenge — Pós-Graduação em Arquitetura de Software (SOAT)

---

## Objetivo

Corrigir as 10 violações da Regra de Dependência identificadas no projeto OficinaMecanica, alinhando a solução à Clean Architecture conforme descrita por Robert C. Martin. O critério é máxima aderência à cartilha.

---

## Decisões arquiteturais

| Decisão | Escolha |
|---|---|
| Estrutura de use cases | Interface por use case (`IAbrirOrdemServicoUseCase : IUseCase<TReq, TRes>`) |
| Domain Events | Implementação própria sem MediatR |
| Value Objects | Implementar agora (`Email` e `Documento` como `sealed record`) |

---

## Estrutura de camadas — o que muda

A hierarquia de projetos `Domain <- Application <- Infrastructure <- API` permanece. O que muda é o conteúdo interno de cada camada.

### Domain (novas subpastas)

```
Domain/
├── Common/
│   ├── IDomainEvent.cs
│   └── Entity.cs          ← classe base que acumula eventos
├── Events/
│   └── OrcamentoEnviadoEvent.cs
└── ValueObjects/
    ├── Email.cs
    ├── Documento.cs
    └── TipoDocumento.cs   ← enum associado ao Documento
```

`OrdemServico` passa a herdar de `Entity`. `Cliente` passa a usar `Email` e `Documento` em vez de `string`.

### Application (reorganização completa)

```
Application/
├── Common/
│   ├── IUseCase.cs                ← interface genérica base
│   ├── Result.cs                  ← Result<T> + ResultErrorType
│   ├── IDomainEventDispatcher.cs
│   └── IEventHandler.cs
├── Configuration/
│   ├── IJwtSettings.cs
│   └── IPasswordSettings.cs
├── DTOs/
│   ├── Requests/                  ← input dos use cases
│   └── Responses/                 ← output dos use cases
├── EventHandlers/
│   └── EnviarEmailOrcamentoHandler.cs
├── Interfaces/
│   ├── ITokenGenerator.cs         ← substitui IJwtService
│   ├── IPasswordHasher.cs
│   ├── IAppLogger.cs
│   └── INotificacaoService.cs     ← permanece
├── Mappers/
│   ├── OrdemServicoMapper.cs
│   ├── ClienteMapper.cs
│   └── ...
└── UseCases/
    ├── OrdemServico/
    │   ├── AbrirOrdemServico/
    │   │   ├── IAbrirOrdemServicoUseCase.cs
    │   │   └── AbrirOrdemServicoUseCase.cs
    │   ├── ConsultarOrdemServico/
    │   ├── ListarOrdensServico/
    │   ├── AdicionarItensOS/
    │   ├── RemoverItemOS/
    │   └── ObterTempoMedioExecucao/
    ├── Cliente/
    │   ├── CriarCliente/
    │   ├── AtualizarCliente/
    │   └── ...
    ├── OrdemServicoStatus/
    │   ├── IniciarDiagnostico/
    │   ├── AprovarOS/
    │   ├── RejeitarOS/
    │   ├── NotificarConclusao/
    │   ├── EntregarOS/
    │   ├── ForcarStatusOS/
    │   └── ObterHistoricoOS/
    ├── Auth/
    │   ├── AutenticarUsuario/
    │   └── RegistrarUsuario/
    ├── Peca/
    ├── Servico/
    └── Veiculo/
```

A pasta `Services/` é removida integralmente ao final da refatoração.

### Infrastructure (novas implementações)

```
Infrastructure/
├── Auth/
│   └── JwtTokenGenerator.cs       ← implementa ITokenGenerator
├── Security/
│   └── Argon2PasswordHasher.cs    ← implementa IPasswordHasher
├── Logging/
│   └── AppLogger.cs               ← implementa IAppLogger<T>
├── Events/
│   └── DomainEventDispatcher.cs   ← implementa IDomainEventDispatcher
└── Notifications/
    └── NotificacaoService.cs      ← migrado de Application/Services/
```

`NotificacaoService.cs` está hoje em `Application/Services/` e depende de lógica externa (envio de e-mail). Por isso migra para Infrastructure, mantendo `INotificacaoService` em `Application/Interfaces/`.

### API (ajustes mínimos)

```
API/
└── Configuration/
    ├── JwtSettings.cs             ← implementa IJwtSettings, lê IConfiguration
    └── PasswordSettings.cs        ← implementa IPasswordSettings, lê IConfiguration
```

Controllers refatorados para injetar interfaces de use case e usar `MapError(result)`.

---

## Detalhamento por violação

### Violação 1 — IConfiguration na Application

`IJwtSettings` e `IPasswordSettings` declaradas em `Application/Configuration/`. A API implementa ambas lendo `IConfiguration`. `JwtService` e `UsuarioService` param de receber `IConfiguration`.

```csharp
// Application/Configuration/IJwtSettings.cs
public interface IJwtSettings
{
    string SecretKey { get; }
    string Issuer { get; }
    string Audience { get; }
    int ExpiracaoMinutos { get; }
}

// Application/Configuration/IPasswordSettings.cs
public interface IPasswordSettings
{
    string PasswordKey { get; }
}
```

**Marco de validação:** `Application.csproj` sem `Microsoft.Extensions.Configuration.Abstractions`.

### Violação 2 — Biblioteca JWT na Application

`JwtService` é removida da Application. Toda a lógica JWT migra para `Infrastructure/Auth/JwtTokenGenerator.cs`, que implementa `ITokenGenerator`.

```csharp
// Application/Interfaces/ITokenGenerator.cs
public interface ITokenGenerator
{
    TokenResponse GerarParaUsuario(Usuario usuario);
}
```

**Marco de validação:** `Application.csproj` sem `Microsoft.IdentityModel.Tokens` nem `System.IdentityModel.Tokens.Jwt`.

### Violação 3 — Biblioteca Argon2 na Application

Toda a lógica de hash (Argon2id + HMAC) migra de `UsuarioService` para `Infrastructure/Security/Argon2PasswordHasher.cs`, que implementa `IPasswordHasher`.

```csharp
// Application/Interfaces/IPasswordHasher.cs
public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
}
```

**Marco de validação:** `Application.csproj` sem `Konscious.Security.Cryptography.Argon2`.

### Violação 4 — ILogger na Application

`IAppLogger<T>` declarada na Application. `AppLogger<T>` na Infrastructure wrapa `ILogger<T>` do ASP.NET.

```csharp
// Application/Interfaces/IAppLogger.cs
public interface IAppLogger<T>
{
    void Info(string message, params object[] args);
    void Warning(string message, Exception? ex = null, params object[] args);
    void Error(string message, Exception? ex = null, params object[] args);
}
```

Registro no DI com tipo aberto: `services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>))`.

### Violação 5 — Exceptions como controle de fluxo

`Result<T>` com `ResultErrorType` em `Application/Common/`. Todos os use cases retornam `Result<T>`. Controllers usam `MapError(result)` centralizado em vez de blocos `try/catch` por endpoint.

```csharp
// Application/Common/Result.cs
public enum ResultErrorType { None, Validation, NotFound, Conflict, Unauthorized }

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, T? value, string? error, ResultErrorType errorType) { ... }

    public static Result<T> Success(T value) => new(true, value, null, ResultErrorType.None);
    public static Result<T> Validation(string error) => new(false, default, error, ResultErrorType.Validation);
    public static Result<T> NotFound(string error) => new(false, default, error, ResultErrorType.NotFound);
    public static Result<T> Conflict(string error) => new(false, default, error, ResultErrorType.Conflict);
    public static Result<T> Unauthorized(string error) => new(false, default, error, ResultErrorType.Unauthorized);
}
```

Exceptions continuam válidas para invariantes de entidade (bugs, não fluxo de negócio).

### Violação 6 — DTOs Request/Response misturados

Reorganização estrutural sem mudança de comportamento:

```
DTOs/Requests/   ← sufixo Request  (ex: AbrirOrdemServicoRequest)
DTOs/Responses/  ← sufixo Response (ex: OrdemServicoResponse)
```

### Violação 7 — Value Objects ausentes

`Email` e `Documento` como `sealed record` em `Domain/ValueObjects/`. A validação migra de métodos estáticos em `Cliente` para os construtores dos VOs.

```csharp
// Domain/ValueObjects/Email.cs
public sealed record Email
{
    public string Valor { get; }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) ||
            !Regex.IsMatch(valor, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Email inválido.", nameof(valor));
        Valor = valor.ToLower().Trim();
    }

    public override string ToString() => Valor;
    public static implicit operator string(Email e) => e.Valor;
}

// Domain/ValueObjects/Documento.cs
public sealed record Documento
{
    public string Valor { get; }
    public TipoDocumento Tipo { get; }

    public Documento(string valor)
    {
        var limpo = Limpar(valor);
        if      (limpo.Length == 11 && ValidarCpf(limpo))  Tipo = TipoDocumento.Cpf;
        else if (limpo.Length == 14 && ValidarCnpj(limpo)) Tipo = TipoDocumento.Cnpj;
        else throw new ArgumentException("CPF ou CNPJ inválido.", nameof(valor));
        Valor = limpo;
    }
    // lógicas ValidarCpf, ValidarCnpj migradas de Cliente
}
```

EF Core usa `ValueConverter` — nenhuma migration necessária pois as colunas continuam `varchar`.

### Violação 8 — Services agregando vários use cases

Cada método de `*Service` vira um use case independente com interface própria:

```csharp
// Application/Common/IUseCase.cs
public interface IUseCase<TRequest, TResponse>
{
    Task<Result<TResponse>> ExecutarAsync(TRequest request);
}

// Interface específica
public interface IAbrirOrdemServicoUseCase
    : IUseCase<AbrirOrdemServicoRequest, OrdemServicoResponse> { }

// Implementação
public class AbrirOrdemServicoUseCase : IAbrirOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;
    private readonly IDomainEventDispatcher _dispatcher;

    public async Task<Result<OrdemServicoResponse>> ExecutarAsync(AbrirOrdemServicoRequest request)
    {
        if (request.ClienteId == Guid.Empty)
            return Result<OrdemServicoResponse>.Validation("ClienteId é obrigatório.");
        if (request.VeiculoId == Guid.Empty)
            return Result<OrdemServicoResponse>.Validation("VeiculoId é obrigatório.");

        var os = new OrdemServico(request.ClienteId, request.VeiculoId, request.Observacoes);
        var id = await _repository.CriarAsync(os);
        var criada = await _repository.ObterPorIdComItensAsync(id);
        return Result<OrdemServicoResponse>.Success(_mapper.MapToResponse(criada!));
    }
}
```

**Marco de validação:** nenhum arquivo `*Service.cs` em `Application/Services/` ao final.

### Violação 9 — Domain Events ausentes

```csharp
// Domain/Common/IDomainEvent.cs
public interface IDomainEvent { DateTime OcorridoEm { get; } }

// Domain/Common/Entity.cs
public abstract class Entity
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();
    protected void RaiseEvent(IDomainEvent evt) => _events.Add(evt);
    public void ClearEvents() => _events.Clear();
}

// Domain/Events/OrcamentoEnviadoEvent.cs
public record OrcamentoEnviadoEvent(
    Guid OrdemServicoId,
    string EmailCliente,
    decimal Total,
    DateTime OcorridoEm) : IDomainEvent;
```

`OrdemServico` herda de `Entity` e levanta `OrcamentoEnviadoEvent` dentro de `EnviarParaAprovacao`.

```csharp
// Application/Common/IEventHandler.cs
public interface IEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent evt);
}

// Application/EventHandlers/EnviarEmailOrcamentoHandler.cs
public class EnviarEmailOrcamentoHandler : IEventHandler<OrcamentoEnviadoEvent>
{
    private readonly INotificacaoService _notificacao;
    public async Task HandleAsync(OrcamentoEnviadoEvent evt) =>
        await _notificacao.EnviarOrcamentoAsync(evt.OrdemServicoId, evt.EmailCliente, evt.Total);
}

// Infrastructure/Events/DomainEventDispatcher.cs
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _provider;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events)
    {
        foreach (var evt in events)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(evt.GetType());
            foreach (var handler in _provider.GetServices(handlerType))
                await ((dynamic)handler).HandleAsync((dynamic)evt);
        }
    }
}
```

### Violação 10 — Mappers dentro do use case

Mappers extraídos para `Application/Mappers/`, registrados como singleton (stateless). Cada use case recebe o mapper por injeção.

```csharp
// Application/Mappers/OrdemServicoMapper.cs
public class OrdemServicoMapper
{
    public OrdemServicoResponse MapToResponse(OrdemServico os) => new() { ... };
    public OrdemServicoResumoResponse MapToResumoResponse(OrdemServico os) => new() { ... };
    public OrdemServicoItemResponse MapToItemResponse(OrdemServicoItem item) => new() { ... };
}
```

---

## Ordem de execução

| Fase | Violações | Critério de conclusão |
|---|---|---|
| 1 — Limpeza estrutural | 1, 2, 3, 4 | `Application.csproj` sem os 4 pacotes externos |
| 2 — Reorganização | 8, 10, 6 | Nenhum `*Service.cs` em `Application/Services/` |
| 3 — Result pattern | 5 | Nenhum `try/catch` de negócio nos controllers |
| 4 — Domain e Value Objects | 9, 7 | `OrdemServico` herda de `Entity`; `Cliente` usa VOs |

---

## Impacto nos testes

- Testes unitários de `*Service` precisam ser renomeados e ajustados para os novos use cases
- `JwtServiceTests` migra para `JwtTokenGeneratorTests` em `Tests.Unit` ou `Tests.Integration`
- Testes de `Cliente` precisam passar `Email` e `Documento` em vez de `string` diretamente
- Testes de integração dos controllers continuam válidos — a interface HTTP não muda

---

## Marcos de validação

1. `Application.csproj` sem `Konscious.Argon2`, `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.IdentityModel.Tokens`, `System.IdentityModel.Tokens.Jwt`
2. Nenhum arquivo `*Service.cs` em `Application/Services/`
3. Nenhum `try/catch` para erros de negócio nos controllers
4. `dotnet build` sem erros após cada fase
5. Suite de testes passando ao final

---

## Referências

- Robert C. Martin — *Clean Architecture* (2017), caps. 5, 11, 20, 22
- Eric Evans — *Domain-Driven Design* (2004), caps. Value Objects e Domain Events
- Vladimir Khorikov — *Enterprise Craftsmanship* (Result pattern, Value Objects em C#)
