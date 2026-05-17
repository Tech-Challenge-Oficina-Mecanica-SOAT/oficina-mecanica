# Refatoração Clean Architecture — Plano de Implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Corrigir as 10 violações de Clean Architecture do projeto OficinaMecanica, alinhando todas as camadas à Regra de Dependência de Uncle Bob.

**Architecture:** Settings via interface declarada na Application, JWT/Argon2/Logger migrados para Infrastructure, Services monolíticos quebrados em Use Cases (interface por caso de uso), Result pattern em vez de exceptions, Value Objects para Email e Documento, Domain Events com despachante próprio.

**Tech Stack:** .NET 10, EF Core 10, xUnit + Moq + FluentAssertions, PostgreSQL.

**Spec:** `docs/superpowers/specs/2026-05-16-clean-architecture-refatoracao-design.md`

---

## Convenções gerais

- Ao final de cada task, rode `dotnet build` na raiz e `dotnet test` no projeto afetado.
- Cada task termina com um commit. Nomeie commits com prefixo `refactor:` ou `feat:`.
- Para mocks, use `Moq` (já presente em `Tests.Unit`).
- Para asserts, use `FluentAssertions`.
- Caminhos absolutos baseados em `/home/viniciusanjos/development/pessoal/oficina-mecanica/`.

---

# FASE 1 — Limpeza estrutural (Violações 1-4)

## Task 1: Settings abstractions

**Objetivo:** Substituir `IConfiguration` na Application por interfaces próprias `IJwtSettings` e `IPasswordSettings`. As implementações ficam na API e leem `IConfiguration` lá.

**Files:**
- Create: `src/OficinaMecanica.Application/Configuration/IJwtSettings.cs`
- Create: `src/OficinaMecanica.Application/Configuration/IPasswordSettings.cs`
- Create: `src/OficinaMecanica.API/Configuration/JwtSettings.cs`
- Create: `src/OficinaMecanica.API/Configuration/PasswordSettings.cs`
- Modify: `src/OficinaMecanica.Application/Services/JwtService.cs` (usar `IJwtSettings`)
- Modify: `src/OficinaMecanica.Application/Services/UsuarioService.cs` (usar `IPasswordSettings`)
- Modify: `src/OficinaMecanica.API/Program.cs` (registrar settings)
- Modify: `src/OficinaMecanica.Application/OficinaMecanica.Application.csproj` (remover pacote `Microsoft.Extensions.Configuration.Abstractions`)
- Test: `tests/OficinaMecanica.Tests.Unit/Services/JwtServiceTests.cs` (atualizar para usar mock de `IJwtSettings`)

- [ ] **Step 1: Criar `IJwtSettings`**

`src/OficinaMecanica.Application/Configuration/IJwtSettings.cs`:
```csharp
namespace OficinaMecanica.Application.Configuration;

public interface IJwtSettings
{
    string SecretKey { get; }
    string Issuer { get; }
    string Audience { get; }
    int ExpiracaoMinutos { get; }
}
```

- [ ] **Step 2: Criar `IPasswordSettings`**

`src/OficinaMecanica.Application/Configuration/IPasswordSettings.cs`:
```csharp
namespace OficinaMecanica.Application.Configuration;

public interface IPasswordSettings
{
    string PasswordKey { get; }
}
```

- [ ] **Step 3: Criar implementação `JwtSettings` na API**

`src/OficinaMecanica.API/Configuration/JwtSettings.cs`:
```csharp
using Microsoft.Extensions.Configuration;
using OficinaMecanica.Application.Configuration;

namespace OficinaMecanica.API.Configuration;

public class JwtSettings : IJwtSettings
{
    public string SecretKey { get; }
    public string Issuer { get; }
    public string Audience { get; }
    public int ExpiracaoMinutos { get; }

    public JwtSettings(IConfiguration configuration)
    {
        SecretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey não configurada.");
        Issuer = configuration["Jwt:Issuer"] ?? "mecanica-api";
        Audience = configuration["Jwt:Audience"] ?? "mecanica-cliente";
        ExpiracaoMinutos = int.TryParse(configuration["Jwt:ExpiracaoMinutos"], out var min) ? min : 5;
    }
}
```

- [ ] **Step 4: Criar implementação `PasswordSettings` na API**

`src/OficinaMecanica.API/Configuration/PasswordSettings.cs`:
```csharp
using Microsoft.Extensions.Configuration;
using OficinaMecanica.Application.Configuration;

namespace OficinaMecanica.API.Configuration;

public class PasswordSettings : IPasswordSettings
{
    public string PasswordKey { get; }

    public PasswordSettings(IConfiguration configuration)
    {
        PasswordKey = configuration["Seguranca:PasswordKey"]
            ?? throw new InvalidOperationException("Seguranca:PasswordKey não configurada.");
    }
}
```

- [ ] **Step 5: Refatorar `JwtService` para usar `IJwtSettings`**

`src/OficinaMecanica.Application/Services/JwtService.cs`:
```csharp
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Application.Configuration;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OficinaMecanica.Application.Services;

public class JwtService : IJwtService
{
    private readonly IJwtSettings _settings;

    public JwtService(IJwtSettings settings) => _settings = settings;

    public TokenDto GerarToken(Usuario usuario)
    {
        var expiracao = DateTime.UtcNow.AddMinutes(_settings.ExpiracaoMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: creds);

        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiracao);
    }
}
```

- [ ] **Step 6: Refatorar `UsuarioService` para usar `IPasswordSettings`**

Substituir o construtor de `src/OficinaMecanica.Application/Services/UsuarioService.cs`:
```csharp
public UsuarioService(IUsuarioRepository repository, IPasswordSettings passwordSettings)
{
    _repository = repository;
    _passwordKey = Encoding.UTF8.GetBytes(passwordSettings.PasswordKey);
}
```

E remover o `using Microsoft.Extensions.Configuration;`. Adicionar `using OficinaMecanica.Application.Configuration;`.

- [ ] **Step 7: Atualizar `Program.cs` para registrar os settings**

Em `src/OficinaMecanica.API/Program.cs`, adicionar antes dos registros de Repository:
```csharp
using OficinaMecanica.API.Configuration;
using OficinaMecanica.Application.Configuration;

// ...
builder.Services.AddSingleton<IJwtSettings, JwtSettings>();
builder.Services.AddSingleton<IPasswordSettings, PasswordSettings>();
```

- [ ] **Step 8: Atualizar `JwtServiceTests` para mockar `IJwtSettings`**

`tests/OficinaMecanica.Tests.Unit/Services/JwtServiceTests.cs`:
```csharp
using Moq;
using OficinaMecanica.Application.Configuration;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OficinaMecanica.Tests.Unit.Services;

public class JwtServiceTests
{
    private readonly JwtService _sut;

    public JwtServiceTests()
    {
        var settings = new Mock<IJwtSettings>();
        settings.Setup(s => s.SecretKey).Returns("mecanica-jwt-secret-key-minimo-32-chars!!");
        settings.Setup(s => s.Issuer).Returns("mecanica-api");
        settings.Setup(s => s.Audience).Returns("mecanica-cliente");
        settings.Setup(s => s.ExpiracaoMinutos).Returns(5);

        _sut = new JwtService(settings.Object);
    }

    // ... resto dos testes existentes permanecem iguais
}
```

Manter os métodos `[Fact]` existentes sem alteração.

- [ ] **Step 9: Remover pacote `Microsoft.Extensions.Configuration.Abstractions`**

Em `src/OficinaMecanica.Application/OficinaMecanica.Application.csproj`, remover:
```xml
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.6" />
```

- [ ] **Step 10: Validar build e testes**

```bash
cd /home/viniciusanjos/development/pessoal/oficina-mecanica
dotnet build
dotnet test tests/OficinaMecanica.Tests.Unit/
```

Esperado: build OK e todos os testes passando.

- [ ] **Step 11: Commit**

```bash
git add src/OficinaMecanica.Application/Configuration/ \
        src/OficinaMecanica.API/Configuration/ \
        src/OficinaMecanica.Application/Services/JwtService.cs \
        src/OficinaMecanica.Application/Services/UsuarioService.cs \
        src/OficinaMecanica.Application/OficinaMecanica.Application.csproj \
        src/OficinaMecanica.API/Program.cs \
        tests/OficinaMecanica.Tests.Unit/Services/JwtServiceTests.cs
git commit -m "refactor: substitui IConfiguration por IJwtSettings e IPasswordSettings (violação 1)"
```

---

## Task 2: Mover JWT para Infrastructure

**Objetivo:** Remover dependência de `Microsoft.IdentityModel.Tokens` e `System.IdentityModel.Tokens.Jwt` da Application. Criar `ITokenGenerator` na Application e mover a implementação para `Infrastructure.Auth.JwtTokenGenerator`.

**Files:**
- Create: `src/OficinaMecanica.Application/Interfaces/ITokenGenerator.cs`
- Create: `src/OficinaMecanica.Infrastructure/Auth/JwtTokenGenerator.cs`
- Delete: `src/OficinaMecanica.Application/Services/JwtService.cs`
- Delete: `src/OficinaMecanica.Application/Interfaces/IJwtService.cs`
- Modify: `src/OficinaMecanica.Application/OficinaMecanica.Application.csproj` (remover pacotes JWT)
- Modify: `src/OficinaMecanica.Infrastructure/OficinaMecanica.Infrastructure.csproj` (adicionar pacotes JWT)
- Modify: `src/OficinaMecanica.API/Program.cs` (atualizar DI)
- Modify: `src/OficinaMecanica.API/Controllers/AuthController.cs` (injetar `ITokenGenerator` em vez de `IJwtService`)
- Modify: `tests/OficinaMecanica.Tests.Unit/OficinaMecanica.Tests.Unit.csproj` (adicionar referência ao projeto Infrastructure)
- Move: `tests/OficinaMecanica.Tests.Unit/Services/JwtServiceTests.cs` → `tests/OficinaMecanica.Tests.Unit/Infrastructure/Auth/JwtTokenGeneratorTests.cs`

- [ ] **Step 1: Criar `ITokenGenerator`**

`src/OficinaMecanica.Application/Interfaces/ITokenGenerator.cs`:
```csharp
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Interfaces;

public interface ITokenGenerator
{
    TokenDto GerarParaUsuario(Usuario usuario);
}
```

- [ ] **Step 2: Adicionar pacotes JWT ao Infrastructure.csproj**

`src/OficinaMecanica.Infrastructure/OficinaMecanica.Infrastructure.csproj`, adicionar dentro do `<ItemGroup>` de pacotes:
```xml
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.18.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.18.0" />
```

- [ ] **Step 3: Criar `JwtTokenGenerator` em Infrastructure**

`src/OficinaMecanica.Infrastructure/Auth/JwtTokenGenerator.cs`:
```csharp
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Application.Configuration;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OficinaMecanica.Infrastructure.Auth;

public class JwtTokenGenerator : ITokenGenerator
{
    private readonly IJwtSettings _settings;

    public JwtTokenGenerator(IJwtSettings settings) => _settings = settings;

    public TokenDto GerarParaUsuario(Usuario usuario)
    {
        var expiracao = DateTime.UtcNow.AddMinutes(_settings.ExpiracaoMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: creds);

        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiracao);
    }
}
```

- [ ] **Step 4: Adicionar referência ao Infrastructure no projeto de testes Unit**

`tests/OficinaMecanica.Tests.Unit/OficinaMecanica.Tests.Unit.csproj`, dentro do `ItemGroup` de `ProjectReference`:
```xml
<ProjectReference Include="..\..\src\OficinaMecanica.Infrastructure\OficinaMecanica.Infrastructure.csproj" />
```

- [ ] **Step 5: Mover e renomear o teste**

Mover `tests/OficinaMecanica.Tests.Unit/Services/JwtServiceTests.cs` para `tests/OficinaMecanica.Tests.Unit/Infrastructure/Auth/JwtTokenGeneratorTests.cs` e atualizar o conteúdo:

```csharp
using Moq;
using OficinaMecanica.Application.Configuration;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;
using OficinaMecanica.Infrastructure.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OficinaMecanica.Tests.Unit.Infrastructure.Auth;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _sut;

    public JwtTokenGeneratorTests()
    {
        var settings = new Mock<IJwtSettings>();
        settings.Setup(s => s.SecretKey).Returns("mecanica-jwt-secret-key-minimo-32-chars!!");
        settings.Setup(s => s.Issuer).Returns("mecanica-api");
        settings.Setup(s => s.Audience).Returns("mecanica-cliente");
        settings.Setup(s => s.ExpiracaoMinutos).Returns(5);

        _sut = new JwtTokenGenerator(settings.Object);
    }

    [Fact]
    public void GerarParaUsuario_RetornaTokenNaoVazio()
    {
        var usuario = new Usuario("test@oficina.com", "hash", Perfil.Admin);
        var resultado = _sut.GerarParaUsuario(usuario);
        Assert.NotNull(resultado.Token);
        Assert.NotEmpty(resultado.Token);
    }

    [Fact]
    public void GerarParaUsuario_ExpiracaoEm5Minutos()
    {
        var usuario = new Usuario("test@oficina.com", "hash", Perfil.Admin);
        var antes = DateTime.UtcNow;
        var resultado = _sut.GerarParaUsuario(usuario);
        Assert.True(resultado.Expiracao > antes.AddMinutes(4));
        Assert.True(resultado.Expiracao < antes.AddMinutes(6));
    }

    [Fact]
    public void GerarParaUsuario_ContemClaimEmail()
    {
        var usuario = new Usuario("claims@oficina.com", "hash", Perfil.Admin);
        var resultado = _sut.GerarParaUsuario(usuario);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Token);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email
                                      && c.Value == "claims@oficina.com");
    }

    [Fact]
    public void GerarParaUsuario_ContemClaimRole()
    {
        var usuario = new Usuario("role@oficina.com", "hash", Perfil.Admin);
        var resultado = _sut.GerarParaUsuario(usuario);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Token);
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GerarParaUsuario_ContemClaimSub()
    {
        var usuario = new Usuario("sub@oficina.com", "hash", Perfil.Admin);
        var resultado = _sut.GerarParaUsuario(usuario);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Token);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub
                                      && c.Value == usuario.Id.ToString());
    }

    [Fact]
    public void GerarParaUsuario_TokensDiferentesParaUsuariosDiferentes()
    {
        var u1 = new Usuario("u1@oficina.com", "hash", Perfil.Admin);
        var u2 = new Usuario("u2@oficina.com", "hash", Perfil.Mecanico);
        var t1 = _sut.GerarParaUsuario(u1).Token;
        var t2 = _sut.GerarParaUsuario(u2).Token;
        Assert.NotEqual(t1, t2);
    }
}
```

- [ ] **Step 6: Atualizar `AuthController` para injetar `ITokenGenerator`**

Em `src/OficinaMecanica.API/Controllers/AuthController.cs`, trocar `IJwtService` por `ITokenGenerator` no construtor e nos campos. Substituir chamadas a `GerarToken(usuario)` por `GerarParaUsuario(usuario)`.

(Leia o arquivo primeiro para entender a estrutura atual antes de editar.)

- [ ] **Step 7: Atualizar `Program.cs`**

Em `src/OficinaMecanica.API/Program.cs`:

Remover:
```csharp
builder.Services.AddScoped<IJwtService, JwtService>();
```

Adicionar:
```csharp
using OficinaMecanica.Infrastructure.Auth;
// ...
builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
```

- [ ] **Step 8: Deletar arquivos antigos**

```bash
rm src/OficinaMecanica.Application/Services/JwtService.cs
rm src/OficinaMecanica.Application/Interfaces/IJwtService.cs
```

- [ ] **Step 9: Remover pacotes JWT do Application.csproj**

Em `src/OficinaMecanica.Application/OficinaMecanica.Application.csproj`, remover:
```xml
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.18.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.18.0" />
```

- [ ] **Step 10: Validar e commitar**

```bash
dotnet build
dotnet test tests/OficinaMecanica.Tests.Unit/
git add -A
git commit -m "refactor: move JWT para Infrastructure via ITokenGenerator (violação 2)"
```

---

## Task 3: Mover Argon2 para Infrastructure

**Objetivo:** Remover `Konscious.Security.Cryptography.Argon2` da Application. Criar `IPasswordHasher` na Application e mover toda a lógica de hash para `Infrastructure.Security.Argon2PasswordHasher`.

**Files:**
- Create: `src/OficinaMecanica.Application/Interfaces/IPasswordHasher.cs`
- Create: `src/OficinaMecanica.Infrastructure/Security/Argon2PasswordHasher.cs`
- Modify: `src/OficinaMecanica.Application/Services/UsuarioService.cs` (usar `IPasswordHasher`)
- Modify: `src/OficinaMecanica.Infrastructure/OficinaMecanica.Infrastructure.csproj` (adicionar Argon2)
- Modify: `src/OficinaMecanica.Application/OficinaMecanica.Application.csproj` (remover Argon2)
- Modify: `src/OficinaMecanica.API/Program.cs` (registrar `IPasswordHasher`)

- [ ] **Step 1: Criar `IPasswordHasher`**

`src/OficinaMecanica.Application/Interfaces/IPasswordHasher.cs`:
```csharp
namespace OficinaMecanica.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
}
```

- [ ] **Step 2: Adicionar pacote Argon2 ao Infrastructure.csproj**

`src/OficinaMecanica.Infrastructure/OficinaMecanica.Infrastructure.csproj`:
```xml
<PackageReference Include="Konscious.Security.Cryptography.Argon2" Version="1.3.1" />
```

- [ ] **Step 3: Criar `Argon2PasswordHasher` em Infrastructure**

`src/OficinaMecanica.Infrastructure/Security/Argon2PasswordHasher.cs`:
```csharp
using Konscious.Security.Cryptography;
using OficinaMecanica.Application.Configuration;
using OficinaMecanica.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace OficinaMecanica.Infrastructure.Security;

public class Argon2PasswordHasher : IPasswordHasher
{
    private const int Argon2Memory = 9216;
    private const int Argon2Iterations = 4;
    private const int Argon2Parallelism = 1;
    private const int Argon2HashLength = 32;
    private const int SaltLength = 32;

    private readonly byte[] _passwordKey;

    public Argon2PasswordHasher(IPasswordSettings settings)
    {
        _passwordKey = Encoding.UTF8.GetBytes(settings.PasswordKey);
    }

    public string Hash(string senha)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltLength);
        byte[] keyedPassword = ComputeHmac(senha);
        byte[] hash = ComputeArgon2id(keyedPassword, salt);

        var b64Salt = Convert.ToBase64String(salt);
        var b64Hash = Convert.ToBase64String(hash);

        return $"$argon2id$v=19$m={Argon2Memory},t={Argon2Iterations},p={Argon2Parallelism}${b64Salt}${b64Hash}";
    }

    public bool Verificar(string senha, string senhaHash)
    {
        var parts = senhaHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5 || parts[0] != "argon2id") return false;

        var paramParts = parts[2].Split(',');
        if (paramParts.Length != 3) return false;

        if (!TryParseArgon2Params(paramParts, out int m, out int t, out int p)) return false;

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expectedHash = Convert.FromBase64String(parts[4]);
        }
        catch { return false; }

        byte[] keyedPassword = ComputeHmac(senha);
        byte[] actualHash = ComputeArgon2id(keyedPassword, salt, m, t, p);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private byte[] ComputeHmac(string senha)
    {
        using var hmac = new HMACSHA256(_passwordKey);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(senha));
    }

    private static byte[] ComputeArgon2id(byte[] password, byte[] salt,
        int m = Argon2Memory, int t = Argon2Iterations, int p = Argon2Parallelism)
    {
        using var argon2 = new Argon2id(password)
        {
            Salt = salt,
            MemorySize = m,
            Iterations = t,
            DegreeOfParallelism = p
        };
        return argon2.GetBytes(Argon2HashLength);
    }

    private static bool TryParseArgon2Params(string[] parts, out int m, out int t, out int p)
    {
        m = t = p = 0;
        foreach (var part in parts)
        {
            var kv = part.Split('=');
            if (kv.Length != 2 || !int.TryParse(kv[1], out int val)) return false;
            switch (kv[0])
            {
                case "m": m = val; break;
                case "t": t = val; break;
                case "p": p = val; break;
                default: return false;
            }
        }
        return m > 0 && t > 0 && p > 0;
    }
}
```

- [ ] **Step 4: Refatorar `UsuarioService` para usar `IPasswordHasher`**

Substituir `src/OficinaMecanica.Application/Services/UsuarioService.cs` por:
```csharp
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _hasher;

    public UsuarioService(IUsuarioRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        var usuario = await _repository.ObterPorEmailAsync(email.ToLower().Trim());
        if (usuario is null) return null;
        return _hasher.Verificar(senha, usuario.SenhaHash) ? usuario : null;
    }

    public async Task<Usuario> RegistrarAsync(RegistrarUsuarioDto dto)
    {
        var existente = await _repository.ObterPorEmailAsync(dto.Email.ToLower().Trim());
        if (existente is not null)
            throw new InvalidOperationException("Email já cadastrado.");

        var hash = _hasher.Hash(dto.Senha);
        var usuario = new Usuario(dto.Email, hash, dto.Perfil);
        await _repository.AdicionarAsync(usuario);
        return usuario;
    }
}
```

- [ ] **Step 5: Registrar `IPasswordHasher` no Program.cs**

Em `src/OficinaMecanica.API/Program.cs` adicionar:
```csharp
using OficinaMecanica.Infrastructure.Security;
// ...
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
```

- [ ] **Step 6: Remover Argon2 do Application.csproj**

Em `src/OficinaMecanica.Application/OficinaMecanica.Application.csproj`, remover:
```xml
<PackageReference Include="Konscious.Security.Cryptography.Argon2" Version="1.3.1" />
```

- [ ] **Step 7: Criar teste de `Argon2PasswordHasher`**

`tests/OficinaMecanica.Tests.Unit/Infrastructure/Security/Argon2PasswordHasherTests.cs`:
```csharp
using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Configuration;
using OficinaMecanica.Infrastructure.Security;

namespace OficinaMecanica.Tests.Unit.Infrastructure.Security;

public class Argon2PasswordHasherTests
{
    private readonly Argon2PasswordHasher _sut;

    public Argon2PasswordHasherTests()
    {
        var settings = new Mock<IPasswordSettings>();
        settings.Setup(s => s.PasswordKey).Returns("chave-de-teste-com-tamanho-adequado-128bits");
        _sut = new Argon2PasswordHasher(settings.Object);
    }

    [Fact]
    public void Hash_GeraStringNaoVazia()
    {
        var resultado = _sut.Hash("senha123");
        resultado.Should().NotBeNullOrEmpty();
        resultado.Should().StartWith("$argon2id$");
    }

    [Fact]
    public void Hash_GeraValoresDiferentesParaMesmaSenha()
    {
        var h1 = _sut.Hash("senha");
        var h2 = _sut.Hash("senha");
        h1.Should().NotBe(h2);
    }

    [Fact]
    public void Verificar_RetornaTrueParaSenhaCorreta()
    {
        var hash = _sut.Hash("senha-correta");
        _sut.Verificar("senha-correta", hash).Should().BeTrue();
    }

    [Fact]
    public void Verificar_RetornaFalseParaSenhaErrada()
    {
        var hash = _sut.Hash("senha-correta");
        _sut.Verificar("senha-errada", hash).Should().BeFalse();
    }

    [Fact]
    public void Verificar_RetornaFalseParaHashMalformado()
    {
        _sut.Verificar("qualquer", "lixo-invalido").Should().BeFalse();
    }
}
```

- [ ] **Step 8: Validar e commitar**

```bash
dotnet build
dotnet test tests/OficinaMecanica.Tests.Unit/
git add -A
git commit -m "refactor: move Argon2 para Infrastructure via IPasswordHasher (violação 3)"
```

---

## Task 4: IAppLogger e migração do NotificacaoService

**Objetivo:** Criar abstração `IAppLogger<T>` na Application, implementação `AppLogger<T>` em Infrastructure, mover `NotificacaoService` para Infrastructure e refatorar `OrdemServicoStatusService` para usar `IAppLogger`.

**Files:**
- Create: `src/OficinaMecanica.Application/Interfaces/IAppLogger.cs`
- Create: `src/OficinaMecanica.Infrastructure/Logging/AppLogger.cs`
- Create: `src/OficinaMecanica.Infrastructure/Notifications/NotificacaoService.cs`
- Delete: `src/OficinaMecanica.Application/Services/NotificacaoService.cs`
- Modify: `src/OficinaMecanica.Application/Services/OrdemServicoStatusService.cs` (usar `IAppLogger`)
- Modify: `src/OficinaMecanica.API/Program.cs` (registrar AppLogger e atualizar NotificacaoService)
- Modify: `tests/OficinaMecanica.Tests.Unit/Services/NotificacaoServiceTests.cs` (atualizar namespace)

- [ ] **Step 1: Criar `IAppLogger`**

`src/OficinaMecanica.Application/Interfaces/IAppLogger.cs`:
```csharp
namespace OficinaMecanica.Application.Interfaces;

public interface IAppLogger<T>
{
    void Info(string message, params object[] args);
    void Warning(string message, Exception? ex = null, params object[] args);
    void Error(string message, Exception? ex = null, params object[] args);
}
```

- [ ] **Step 2: Criar `AppLogger` em Infrastructure**

`src/OficinaMecanica.Infrastructure/Logging/AppLogger.cs`:
```csharp
using Microsoft.Extensions.Logging;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.Infrastructure.Logging;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public AppLogger(ILogger<T> logger) => _logger = logger;

    public void Info(string message, params object[] args) =>
        _logger.LogInformation(message, args);

    public void Warning(string message, Exception? ex = null, params object[] args) =>
        _logger.LogWarning(ex, message, args);

    public void Error(string message, Exception? ex = null, params object[] args) =>
        _logger.LogError(ex, message, args);
}
```

- [ ] **Step 3: Mover `NotificacaoService` para Infrastructure**

`src/OficinaMecanica.Infrastructure/Notifications/NotificacaoService.cs`:
```csharp
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.Infrastructure.Notifications;

public class NotificacaoService : INotificacaoService
{
    private readonly IAppLogger<NotificacaoService> _logger;

    public NotificacaoService(IAppLogger<NotificacaoService> logger) => _logger = logger;

    public Task EnviarOrcamentoAsync(Guid osId, string emailCliente, decimal totalOrcamento)
    {
        _logger.Info(
            "Orcamento enviado. OS: {OsId}, Cliente: {Email}, Total: {Total}",
            osId, emailCliente, totalOrcamento);

        return Task.CompletedTask;
    }

    public Task EnviarConclusaoAsync(Guid osId, string emailCliente)
    {
        _logger.Info(
            "Notificacao de conclusao enviada. OS: {OsId}, Cliente: {Email}",
            osId, emailCliente);

        return Task.CompletedTask;
    }
}
```

- [ ] **Step 4: Refatorar `OrdemServicoStatusService` para usar `IAppLogger`**

Em `src/OficinaMecanica.Application/Services/OrdemServicoStatusService.cs`:

Trocar `using Microsoft.Extensions.Logging;` por nada (remover esse using).

Trocar:
```csharp
private readonly ILogger<OrdemServicoStatusService> _logger;

public OrdemServicoStatusService(
    IOrdemServicoRepository osRepository,
    INotificacaoService notificacaoService,
    ILogger<OrdemServicoStatusService> logger)
```

Por:
```csharp
private readonly IAppLogger<OrdemServicoStatusService> _logger;

public OrdemServicoStatusService(
    IOrdemServicoRepository osRepository,
    INotificacaoService notificacaoService,
    IAppLogger<OrdemServicoStatusService> logger)
```

E trocar `_logger.LogWarning(ex, "...")` por `_logger.Warning("...", ex, ...)`.

- [ ] **Step 5: Deletar `NotificacaoService` antigo**

```bash
rm src/OficinaMecanica.Application/Services/NotificacaoService.cs
```

- [ ] **Step 6: Atualizar `Program.cs`**

Em `src/OficinaMecanica.API/Program.cs`:

Adicionar:
```csharp
using OficinaMecanica.Infrastructure.Logging;
using OficinaMecanica.Infrastructure.Notifications;
// ...
builder.Services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));
```

A linha `builder.Services.AddScoped<INotificacaoService, NotificacaoService>();` permanece, mas agora a implementação é o `NotificacaoService` de Infrastructure (resolvido pelo using).

- [ ] **Step 7: Atualizar `NotificacaoServiceTests`**

Em `tests/OficinaMecanica.Tests.Unit/Services/NotificacaoServiceTests.cs`:
- Atualizar `using OficinaMecanica.Application.Services;` para `using OficinaMecanica.Infrastructure.Notifications;`
- Atualizar o mock de `ILogger<NotificacaoService>` para `IAppLogger<NotificacaoService>`

- [ ] **Step 8: Validar e commitar**

```bash
dotnet build
dotnet test tests/OficinaMecanica.Tests.Unit/
git add -A
git commit -m "refactor: cria IAppLogger e move NotificacaoService para Infrastructure (violação 4)"
```

---

# FASE 2 — Base para use cases (Violações 5, 6, 10)

## Task 5: Result<T> e IUseCase

**Objetivo:** Criar a fundação para os use cases: `Result<T>` para retorno sem exceptions, `IUseCase<TReq, TRes>` como contrato base.

**Files:**
- Create: `src/OficinaMecanica.Application/Common/Result.cs`
- Create: `src/OficinaMecanica.Application/Common/IUseCase.cs`
- Test: `tests/OficinaMecanica.Tests.Unit/Common/ResultTests.cs`

- [ ] **Step 1: Escrever testes do `Result<T>`**

`tests/OficinaMecanica.Tests.Unit/Common/ResultTests.cs`:
```csharp
using FluentAssertions;
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.Tests.Unit.Common;

public class ResultTests
{
    [Fact]
    public void Success_RetornaIsSuccessTrueComValor()
    {
        var result = Result<string>.Success("ok");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
        result.ErrorType.Should().Be(ResultErrorType.None);
    }

    [Fact]
    public void Validation_RetornaIsSuccessFalseComMensagem()
    {
        var result = Result<string>.Validation("campo obrigatório");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("campo obrigatório");
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public void NotFound_DefineErrorTypeNotFound()
    {
        var result = Result<int>.NotFound("recurso ausente");
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public void Conflict_DefineErrorTypeConflict()
    {
        var result = Result<int>.Conflict("já existe");
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public void Unauthorized_DefineErrorTypeUnauthorized()
    {
        var result = Result<int>.Unauthorized("sem acesso");
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
    }
}
```

- [ ] **Step 2: Rodar teste e ver falhar**

```bash
dotnet test tests/OficinaMecanica.Tests.Unit/ --filter "FullyQualifiedName~ResultTests"
```

Esperado: erro de compilação (Result e ResultErrorType não existem).

- [ ] **Step 3: Implementar `ResultErrorType` e `Result<T>`**

`src/OficinaMecanica.Application/Common/Result.cs`:
```csharp
namespace OficinaMecanica.Application.Common;

public enum ResultErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Unauthorized
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, T? value, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, null, ResultErrorType.None);
    public static Result<T> Validation(string error) => new(false, default, error, ResultErrorType.Validation);
    public static Result<T> NotFound(string error) => new(false, default, error, ResultErrorType.NotFound);
    public static Result<T> Conflict(string error) => new(false, default, error, ResultErrorType.Conflict);
    public static Result<T> Unauthorized(string error) => new(false, default, error, ResultErrorType.Unauthorized);
}
```

- [ ] **Step 4: Rodar teste e ver passar**

```bash
dotnet test tests/OficinaMecanica.Tests.Unit/ --filter "FullyQualifiedName~ResultTests"
```

- [ ] **Step 5: Criar `IUseCase`**

`src/OficinaMecanica.Application/Common/IUseCase.cs`:
```csharp
namespace OficinaMecanica.Application.Common;

public interface IUseCase<TRequest, TResponse>
{
    Task<Result<TResponse>> ExecutarAsync(TRequest request);
}
```

- [ ] **Step 6: Commit**

```bash
git add src/OficinaMecanica.Application/Common/ tests/OficinaMecanica.Tests.Unit/Common/
git commit -m "feat: adiciona Result<T> e IUseCase como base para use cases"
```

---

## Task 6: Reorganizar DTOs em Requests e Responses

**Objetivo:** Mover DTOs para subpastas `Requests/` e `Responses/` com nomes claros. Os DTOs antigos são removidos e novos arquivos criados com nomes alinhados ao papel (input/output).

**Mapeamento de renomeações:**

Requests:
- `CreateOrdemServicoDto` → `AbrirOrdemServicoRequest`
- `CreateOrdemServicoItemDto` → `AdicionarOSItemRequest`
- `CreateClienteDto` → `CriarClienteRequest`
- `UpdateClienteDto` → `AtualizarClienteRequest`
- `LoginDto` → `LoginRequest`
- `RegistrarUsuarioDto` → `RegistrarUsuarioRequest`
- `CreateVeiculoDto` → `CriarVeiculoRequest`
- `UpdateVeiculoDto` → `AtualizarVeiculoRequest`
- `CreatePecaDto` → `CriarPecaRequest`
- `UpdatePecaDto` → `AtualizarPecaRequest`
- `UpdateEstoqueDto` → `AtualizarEstoqueRequest`
- `CreateServicoDto` → `CriarServicoRequest`
- `UpdateServicoDto` → `AtualizarServicoRequest`
- `RejeitarOSDto` → `RejeitarOSRequest`
- `TransicaoStatusOSDto` → `TransicaoStatusOSRequest`

Responses:
- `OrdemServicoDto` → `OrdemServicoResponse`
- `OrdemServicoResumoDto` → `OrdemServicoResumoResponse`
- `OrdemServicoItemDto` → `OrdemServicoItemResponse`
- `ClienteDto` → `ClienteResponse`
- `VeiculoDto` → `VeiculoResponse`
- `PecaDto` → `PecaResponse`
- `ServicoDto` → `ServicoResponse`
- `TokenDto` → `TokenResponse`
- `HistoricoStatusOSDto` → `HistoricoStatusOSResponse`
- `PainelStatusOSDto` → `PainelStatusOSResponse`

**Files (parcial — siga o mapeamento acima para os demais):**
- Create: `src/OficinaMecanica.Application/DTOs/Requests/AbrirOrdemServicoRequest.cs`
- Create: `src/OficinaMecanica.Application/DTOs/Requests/AdicionarOSItemRequest.cs`
- ... (todos os Requests da lista acima)
- Create: `src/OficinaMecanica.Application/DTOs/Responses/OrdemServicoResponse.cs`
- ... (todos os Responses da lista acima)
- Delete: todos os arquivos antigos em `src/OficinaMecanica.Application/DTOs/*.cs`

- [ ] **Step 1: Criar todos os Requests**

Para cada Request, crie um arquivo no padrão. Exemplo de `AbrirOrdemServicoRequest`:

`src/OficinaMecanica.Application/DTOs/Requests/AbrirOrdemServicoRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class AbrirOrdemServicoRequest
{
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string Observacoes { get; set; } = string.Empty;
}
```

`src/OficinaMecanica.Application/DTOs/Requests/AdicionarOSItemRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class AdicionarOSItemRequest
{
    public string Tipo { get; set; } = string.Empty;
    public Guid ReferenciaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Requests/CriarClienteRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class CriarClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

`src/OficinaMecanica.Application/DTOs/Requests/AtualizarClienteRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class AtualizarClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

`src/OficinaMecanica.Application/DTOs/Requests/LoginRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public record LoginRequest(string Email, string Senha);
```

`src/OficinaMecanica.Application/DTOs/Requests/RegistrarUsuarioRequest.cs`:
```csharp
using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Application.DTOs.Requests;

public record RegistrarUsuarioRequest(string Email, string Senha, Perfil Perfil = Perfil.Admin);
```

`src/OficinaMecanica.Application/DTOs/Requests/CriarVeiculoRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class CriarVeiculoRequest
{
    public Guid ClienteId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Requests/AtualizarVeiculoRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class AtualizarVeiculoRequest
{
    public Guid? ClienteId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Requests/CriarPecaRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class CriarPecaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Estoque { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Requests/AtualizarPecaRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class AtualizarPecaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Estoque { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Requests/AtualizarEstoqueRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class AtualizarEstoqueRequest
{
    public int Quantidade { get; set; }
    public string TipoOperacao { get; set; } = string.Empty;
}
```

`src/OficinaMecanica.Application/DTOs/Requests/CriarServicoRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class CriarServicoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Requests/AtualizarServicoRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class AtualizarServicoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Requests/RejeitarOSRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public record RejeitarOSRequest(string Motivo);
```

`src/OficinaMecanica.Application/DTOs/Requests/TransicaoStatusOSRequest.cs`:
```csharp
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.DTOs.Requests;

public record TransicaoStatusOSRequest(EnumStatusOS NovoStatus, string Motivo);
```

- [ ] **Step 2: Criar todos os Responses**

`src/OficinaMecanica.Application/DTOs/Responses/OrdemServicoItemResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public class OrdemServicoItemResponse
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public Guid ReferenciaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Responses/OrdemServicoResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public class OrdemServicoResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public Guid VeiculoId { get; set; }
    public string VeiculoDescricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public List<OrdemServicoItemResponse> Itens { get; set; } = new();
}
```

`src/OficinaMecanica.Application/DTOs/Responses/OrdemServicoResumoResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public class OrdemServicoResumoResponse
{
    public Guid Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string VeiculoDescricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Responses/ClienteResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public class ClienteResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Responses/VeiculoResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public class VeiculoResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public DateTime CriadoEm { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Responses/PecaResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public class PecaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Estoque { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public bool Ativo { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Responses/ServicoResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public class ServicoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
```

`src/OficinaMecanica.Application/DTOs/Responses/TokenResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public record TokenResponse(string Token, DateTime Expiracao);
```

`src/OficinaMecanica.Application/DTOs/Responses/HistoricoStatusOSResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public record HistoricoStatusOSResponse(
    Guid Id,
    Guid OrdemServicoId,
    string? StatusAnterior,
    string StatusNovo,
    DateTime AlteradoEm,
    string AlteradoPor,
    string? Motivo
);
```

`src/OficinaMecanica.Application/DTOs/Responses/PainelStatusOSResponse.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Responses;

public record PainelStatusOSResponse(
    Guid OsId,
    string Status,
    DateTime AtualizadoEm
);
```

- [ ] **Step 3: Atualizar todos os usos no código existente**

A refatoração dos use cases (Tasks 8-14) já vai consumir os novos DTOs. Por enquanto, atualize os arquivos que ainda usam os antigos para manter o build verde:

- `src/OficinaMecanica.Application/Services/*.cs` — substituir nomes de DTOs (busca/substitui global)
- `src/OficinaMecanica.API/Controllers/*.cs` — substituir nomes de DTOs

Atalho prático com sed (rode da raiz):
```bash
cd /home/viniciusanjos/development/pessoal/oficina-mecanica
find src tests -type f -name "*.cs" | xargs sed -i \
  -e 's/CreateOrdemServicoDto/AbrirOrdemServicoRequest/g' \
  -e 's/CreateOrdemServicoItemDto/AdicionarOSItemRequest/g' \
  -e 's/OrdemServicoDto/OrdemServicoResponse/g' \
  -e 's/OrdemServicoResumoDto/OrdemServicoResumoResponse/g' \
  -e 's/OrdemServicoItemDto/OrdemServicoItemResponse/g' \
  -e 's/CreateClienteDto/CriarClienteRequest/g' \
  -e 's/UpdateClienteDto/AtualizarClienteRequest/g' \
  -e 's/ClienteDto/ClienteResponse/g' \
  -e 's/CreateVeiculoDto/CriarVeiculoRequest/g' \
  -e 's/UpdateVeiculoDto/AtualizarVeiculoRequest/g' \
  -e 's/VeiculoDto/VeiculoResponse/g' \
  -e 's/CreatePecaDto/CriarPecaRequest/g' \
  -e 's/UpdatePecaDto/AtualizarPecaRequest/g' \
  -e 's/UpdateEstoqueDto/AtualizarEstoqueRequest/g' \
  -e 's/PecaDto/PecaResponse/g' \
  -e 's/CreateServicoDto/CriarServicoRequest/g' \
  -e 's/UpdateServicoDto/AtualizarServicoRequest/g' \
  -e 's/ServicoDto/ServicoResponse/g' \
  -e 's/LoginDto/LoginRequest/g' \
  -e 's/RegistrarUsuarioDto/RegistrarUsuarioRequest/g' \
  -e 's/TokenDto/TokenResponse/g' \
  -e 's/RejeitarOSDto/RejeitarOSRequest/g' \
  -e 's/TransicaoStatusOSDto/TransicaoStatusOSRequest/g' \
  -e 's/HistoricoStatusOSDto/HistoricoStatusOSResponse/g' \
  -e 's/PainelStatusOSDto/PainelStatusOSResponse/g'
```

Depois adicione os usings em arquivos que precisarem:
- Serviços: `using OficinaMecanica.Application.DTOs.Requests;` e `using OficinaMecanica.Application.DTOs.Responses;`
- Controllers: idem
- Interfaces de serviços: idem

Atalho:
```bash
find src tests -type f -name "*.cs" -exec grep -l "OficinaMecanica.Application.DTOs;" {} \; | \
  xargs sed -i 's|using OficinaMecanica.Application.DTOs;|using OficinaMecanica.Application.DTOs.Requests;\nusing OficinaMecanica.Application.DTOs.Responses;|'
```

- [ ] **Step 4: Deletar arquivos antigos de DTO**

```bash
rm src/OficinaMecanica.Application/DTOs/ClienteDto.cs
rm src/OficinaMecanica.Application/DTOs/HistoricoStatusOSDto.cs
rm src/OficinaMecanica.Application/DTOs/LoginDto.cs
rm src/OficinaMecanica.Application/DTOs/OrdemServicoDto.cs
rm src/OficinaMecanica.Application/DTOs/PainelStatusOSDto.cs
rm src/OficinaMecanica.Application/DTOs/PecaDto.cs
rm src/OficinaMecanica.Application/DTOs/RegistrarUsuarioDto.cs
rm src/OficinaMecanica.Application/DTOs/RejeitarOSDto.cs
rm src/OficinaMecanica.Application/DTOs/ServicoDto.cs
rm src/OficinaMecanica.Application/DTOs/TokenDto.cs
rm src/OficinaMecanica.Application/DTOs/TransicaoStatusOSDto.cs
rm src/OficinaMecanica.Application/DTOs/VeiculoDto.cs
```

- [ ] **Step 5: Verificar testes (renomear arquivos de teste DTOs)**

```bash
mv tests/OficinaMecanica.Tests.Unit/DTOs tests/OficinaMecanica.Tests.Unit/DTOs.Old
```

Recriar pasta `tests/OficinaMecanica.Tests.Unit/DTOs/` e mover testes adaptando ao novo nome se algum ainda fizer sentido. Se forem só testes triviais de propriedades, podem ser descartados.

- [ ] **Step 6: Build e commit**

```bash
dotnet build
dotnet test tests/OficinaMecanica.Tests.Unit/
git add -A
git commit -m "refactor: reorganiza DTOs em Requests/ e Responses/ (violação 6)"
```

---

## Task 7: Mappers

**Objetivo:** Extrair os mapeadores das Services para classes dedicadas em `Application/Mappers/`. Cada entidade tem seu mapper, registrado como singleton.

**Files:**
- Create: `src/OficinaMecanica.Application/Mappers/OrdemServicoMapper.cs`
- Create: `src/OficinaMecanica.Application/Mappers/ClienteMapper.cs`
- Create: `src/OficinaMecanica.Application/Mappers/VeiculoMapper.cs`
- Create: `src/OficinaMecanica.Application/Mappers/PecaMapper.cs`
- Create: `src/OficinaMecanica.Application/Mappers/ServicoMapper.cs`
- Create: `src/OficinaMecanica.Application/Mappers/HistoricoStatusOSMapper.cs`
- Modify: `src/OficinaMecanica.API/Program.cs` (registrar mappers como singletons)

- [ ] **Step 1: `OrdemServicoMapper`**

`src/OficinaMecanica.Application/Mappers/OrdemServicoMapper.cs`:
```csharp
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class OrdemServicoMapper
{
    public OrdemServicoResponse MapToResponse(OrdemServico os) => new()
    {
        Id = os.Id,
        ClienteId = os.ClienteId,
        ClienteNome = os.Cliente?.Nome ?? string.Empty,
        VeiculoId = os.VeiculoId,
        VeiculoDescricao = os.Veiculo != null
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} ({os.Veiculo.Placa})"
            : string.Empty,
        Status = os.StatusOS.ToString(),
        Observacoes = os.Observacoes,
        Total = os.Total,
        DataAbertura = os.DataAbertura,
        DataFechamento = os.DataFechamento,
        Itens = os.Itens.Select(MapToItemResponse).ToList()
    };

    public OrdemServicoResumoResponse MapToResumoResponse(OrdemServico os) => new()
    {
        Id = os.Id,
        ClienteNome = os.Cliente?.Nome ?? string.Empty,
        VeiculoDescricao = os.Veiculo != null
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} ({os.Veiculo.Placa})"
            : string.Empty,
        Status = os.StatusOS.ToString(),
        Total = os.Total,
        DataAbertura = os.DataAbertura,
        DataFechamento = os.DataFechamento
    };

    public OrdemServicoItemResponse MapToItemResponse(OrdemServicoItem item) => new()
    {
        Id = item.Id,
        Tipo = item.Tipo.ToString().ToLower(),
        ReferenciaId = item.ReferenciaId,
        Descricao = item.Descricao,
        Quantidade = item.Quantidade,
        PrecoUnitario = item.PrecoUnitario,
        Subtotal = item.Subtotal
    };
}
```

- [ ] **Step 2: `ClienteMapper`**

`src/OficinaMecanica.Application/Mappers/ClienteMapper.cs`:
```csharp
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class ClienteMapper
{
    public ClienteResponse MapToResponse(Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nome = cliente.Nome,
        Documento = cliente.Documento,
        Telefone = cliente.Telefone,
        Email = cliente.Email,
        Ativo = cliente.Ativo,
        CriadoEm = cliente.CriadoEm
    };
}
```

- [ ] **Step 3: `VeiculoMapper`**

`src/OficinaMecanica.Application/Mappers/VeiculoMapper.cs`:
```csharp
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class VeiculoMapper
{
    public VeiculoResponse MapToResponse(Veiculo veiculo) => new()
    {
        Id = veiculo.Id,
        ClienteId = veiculo.ClienteId,
        ClienteNome = veiculo.Cliente?.Nome ?? string.Empty,
        Placa = veiculo.Placa,
        Marca = veiculo.Marca,
        Modelo = veiculo.Modelo,
        Ano = veiculo.Ano,
        CriadoEm = veiculo.CriadoEm
    };
}
```

- [ ] **Step 4: `PecaMapper`**

`src/OficinaMecanica.Application/Mappers/PecaMapper.cs`:
```csharp
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class PecaMapper
{
    public PecaResponse MapToResponse(PecaInsumo peca) => new()
    {
        Id = peca.Id,
        Nome = peca.Nome,
        Codigo = peca.Codigo,
        Descricao = peca.Descricao,
        PrecoUnitario = peca.Preco,
        Estoque = peca.Quantidade,
        CriadoEm = peca.CriadoEm,
        Ativo = peca.Ativo
    };
}
```

- [ ] **Step 5: `ServicoMapper`**

`src/OficinaMecanica.Application/Mappers/ServicoMapper.cs`:
```csharp
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class ServicoMapper
{
    public ServicoResponse MapToResponse(Servico servico) => new()
    {
        Id = servico.Id,
        Nome = servico.Nome,
        Descricao = servico.Descricao,
        Valor = servico.Valor,
        Ativo = servico.Ativo,
        CriadoEm = servico.CriadoEm
    };
}
```

- [ ] **Step 6: `HistoricoStatusOSMapper`**

`src/OficinaMecanica.Application/Mappers/HistoricoStatusOSMapper.cs`:
```csharp
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class HistoricoStatusOSMapper
{
    public HistoricoStatusOSResponse MapToResponse(HistoricoStatusOS h) => new(
        h.Id,
        h.OrdemServicoId,
        h.StatusAnterior?.ToString(),
        h.StatusNovo.ToString(),
        h.AlteradoEm,
        h.AlteradoPor,
        h.Motivo
    );
}
```

- [ ] **Step 7: Registrar mappers no `Program.cs`**

Em `src/OficinaMecanica.API/Program.cs`:
```csharp
using OficinaMecanica.Application.Mappers;
// ...
builder.Services.AddSingleton<OrdemServicoMapper>();
builder.Services.AddSingleton<ClienteMapper>();
builder.Services.AddSingleton<VeiculoMapper>();
builder.Services.AddSingleton<PecaMapper>();
builder.Services.AddSingleton<ServicoMapper>();
builder.Services.AddSingleton<HistoricoStatusOSMapper>();
```

- [ ] **Step 8: Commit**

```bash
git add src/OficinaMecanica.Application/Mappers/ src/OficinaMecanica.API/Program.cs
git commit -m "feat: extrai mappers para classes dedicadas em Application/Mappers (violação 10)"
```

---

# FASE 3 — Use Cases (Violação 8)

> **Nota geral para Tasks 8-14:** Cada use case segue o padrão:
> 1. Interface `I{Verbo}{Entidade}UseCase : IUseCase<TRequest, TResponse>` em `Application/UseCases/{Entidade}/{NomeDoCaso}/`
> 2. Classe `{Verbo}{Entidade}UseCase` no mesmo diretório
> 3. Retorna `Result<TResponse>` em vez de lançar exceptions
> 4. Recebe repositórios e mapper por injeção
>
> Use cases que retornam "void" lógico (ex.: `Desativar`) usam `Result<bool>` ou um marcador `Unit`. Para este plano, padronize em `Result<bool>` retornando `true` no sucesso.

## Task 8: Use cases de OrdemServico

**Use cases a criar:**
1. `AbrirOrdemServicoUseCase`
2. `ConsultarOrdemServicoUseCase`
3. `ListarOrdensServicoUseCase`
4. `AdicionarItensOSUseCase`
5. `RemoverItemOSUseCase`
6. `ObterTempoMedioExecucaoUseCase`

**Files:**
- Create: `src/OficinaMecanica.Application/UseCases/OrdemServico/AbrirOrdemServico/{IAbrirOrdemServicoUseCase.cs, AbrirOrdemServicoUseCase.cs}`
- Create: idem para os outros 5 casos
- Test: `tests/OficinaMecanica.Tests.Unit/UseCases/OrdemServico/{nome}/{NomeUseCase}Tests.cs` para cada um

- [ ] **Step 1: `AbrirOrdemServicoUseCase` (interface + classe)**

`src/OficinaMecanica.Application/UseCases/OrdemServico/AbrirOrdemServico/IAbrirOrdemServicoUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;

public interface IAbrirOrdemServicoUseCase
    : IUseCase<AbrirOrdemServicoRequest, OrdemServicoResponse> { }
```

`src/OficinaMecanica.Application/UseCases/OrdemServico/AbrirOrdemServico/AbrirOrdemServicoUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;

public class AbrirOrdemServicoUseCase : IAbrirOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;

    public AbrirOrdemServicoUseCase(IOrdemServicoRepository repository, OrdemServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrdemServicoResponse>> ExecutarAsync(AbrirOrdemServicoRequest request)
    {
        if (request.ClienteId == Guid.Empty)
            return Result<OrdemServicoResponse>.Validation("ClienteId é obrigatório.");

        if (request.VeiculoId == Guid.Empty)
            return Result<OrdemServicoResponse>.Validation("VeiculoId é obrigatório.");

        var os = new Domain.Entities.OrdemServico(request.ClienteId, request.VeiculoId, request.Observacoes);
        var id = await _repository.CriarAsync(os);
        var criada = await _repository.ObterPorIdComItensAsync(id);

        return Result<OrdemServicoResponse>.Success(_mapper.MapToResponse(criada!));
    }
}
```

- [ ] **Step 2: Teste de `AbrirOrdemServicoUseCase`**

`tests/OficinaMecanica.Tests.Unit/UseCases/OrdemServico/AbrirOrdemServico/AbrirOrdemServicoUseCaseTests.cs`:
```csharp
using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServico.AbrirOrdemServico;

public class AbrirOrdemServicoUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly AbrirOrdemServicoUseCase _sut;

    public AbrirOrdemServicoUseCaseTests()
    {
        _sut = new AbrirOrdemServicoUseCase(_repo.Object, new OrdemServicoMapper());
    }

    [Fact]
    public async Task ExecutarAsync_ComClienteIdVazio_RetornaValidation()
    {
        var request = new AbrirOrdemServicoRequest { ClienteId = Guid.Empty, VeiculoId = Guid.NewGuid() };
        var result = await _sut.ExecutarAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_ComVeiculoIdVazio_RetornaValidation()
    {
        var request = new AbrirOrdemServicoRequest { ClienteId = Guid.NewGuid(), VeiculoId = Guid.Empty };
        var result = await _sut.ExecutarAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_CriaOS()
    {
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var osId = Guid.NewGuid();
        var criada = new Domain.Entities.OrdemServico(clienteId, veiculoId, "obs");

        _repo.Setup(r => r.CriarAsync(It.IsAny<Domain.Entities.OrdemServico>())).ReturnsAsync(osId);
        _repo.Setup(r => r.ObterPorIdComItensAsync(osId)).ReturnsAsync(criada);

        var request = new AbrirOrdemServicoRequest
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Observacoes = "obs"
        };

        var result = await _sut.ExecutarAsync(request);
        result.IsSuccess.Should().BeTrue();
        result.Value!.ClienteId.Should().Be(clienteId);
    }
}
```

- [ ] **Step 3: `ConsultarOrdemServicoUseCase`**

Para este caso, o request é apenas o `Guid id`. Vamos usar um wrapper:

`src/OficinaMecanica.Application/UseCases/OrdemServico/ConsultarOrdemServico/IConsultarOrdemServicoUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ConsultarOrdemServico;

public interface IConsultarOrdemServicoUseCase
    : IUseCase<Guid, OrdemServicoResponse> { }
```

`src/OficinaMecanica.Application/UseCases/OrdemServico/ConsultarOrdemServico/ConsultarOrdemServicoUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ConsultarOrdemServico;

public class ConsultarOrdemServicoUseCase : IConsultarOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;

    public ConsultarOrdemServicoUseCase(IOrdemServicoRepository repository, OrdemServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrdemServicoResponse>> ExecutarAsync(Guid id)
    {
        var os = await _repository.ObterPorIdComItensAsync(id);
        if (os is null)
            return Result<OrdemServicoResponse>.NotFound("Ordem de serviço não encontrada.");

        return Result<OrdemServicoResponse>.Success(_mapper.MapToResponse(os));
    }
}
```

- [ ] **Step 4: `ListarOrdensServicoUseCase`**

`src/OficinaMecanica.Application/UseCases/OrdemServico/ListarOrdensServico/IListarOrdensServicoUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;

public interface IListarOrdensServicoUseCase
    : IUseCase<Unit, IEnumerable<OrdemServicoResumoResponse>> { }
```

Para use cases sem request, crie um marcador `Unit`:

`src/OficinaMecanica.Application/Common/Unit.cs`:
```csharp
namespace OficinaMecanica.Application.Common;

public readonly record struct Unit
{
    public static Unit Value { get; } = new();
}
```

`src/OficinaMecanica.Application/UseCases/OrdemServico/ListarOrdensServico/ListarOrdensServicoUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;

public class ListarOrdensServicoUseCase : IListarOrdensServicoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;

    public ListarOrdensServicoUseCase(IOrdemServicoRepository repository, OrdemServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<OrdemServicoResumoResponse>>> ExecutarAsync(Unit _)
    {
        var lista = await _repository.ListarTodosAsync();
        return Result<IEnumerable<OrdemServicoResumoResponse>>.Success(lista.Select(_mapper.MapToResumoResponse));
    }
}
```

- [ ] **Step 5: `AdicionarItensOSUseCase`**

Crie um request wrapper para o use case:

`src/OficinaMecanica.Application/DTOs/Requests/AdicionarItensOSRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public class AdicionarItensOSRequest
{
    public Guid OrdemServicoId { get; set; }
    public List<AdicionarOSItemRequest> Itens { get; set; } = new();
}
```

`src/OficinaMecanica.Application/UseCases/OrdemServico/AdicionarItensOS/IAdicionarItensOSUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServico.AdicionarItensOS;

public interface IAdicionarItensOSUseCase
    : IUseCase<AdicionarItensOSRequest, IEnumerable<OrdemServicoItemResponse>> { }
```

`src/OficinaMecanica.Application/UseCases/OrdemServico/AdicionarItensOS/AdicionarItensOSUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.AdicionarItensOS;

public class AdicionarItensOSUseCase : IAdicionarItensOSUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;
    private readonly IMarcarAguardandoAprovacaoUseCase _marcarAguardando;
    private readonly INotificacaoService _notificacao;

    public AdicionarItensOSUseCase(
        IOrdemServicoRepository repository,
        OrdemServicoMapper mapper,
        IMarcarAguardandoAprovacaoUseCase marcarAguardando,
        INotificacaoService notificacao)
    {
        _repository = repository;
        _mapper = mapper;
        _marcarAguardando = marcarAguardando;
        _notificacao = notificacao;
    }

    public async Task<Result<IEnumerable<OrdemServicoItemResponse>>> ExecutarAsync(AdicionarItensOSRequest request)
    {
        var os = await _repository.ObterPorIdComItensAsync(request.OrdemServicoId);
        if (os is null)
            return Result<IEnumerable<OrdemServicoItemResponse>>.NotFound("Ordem de serviço não encontrada.");

        var itens = new List<OrdemServicoItem>();
        foreach (var itemDto in request.Itens)
        {
            if (!Enum.TryParse<TipoOSItem>(itemDto.Tipo, ignoreCase: true, out var tipo))
                return Result<IEnumerable<OrdemServicoItemResponse>>.Validation("Tipo inválido. Use: servico, peca ou insumo.");

            itens.Add(new OrdemServicoItem(
                request.OrdemServicoId,
                tipo,
                itemDto.ReferenciaId,
                itemDto.Descricao,
                itemDto.Quantidade,
                itemDto.PrecoUnitario));
        }

        var salvos = await _repository.AdicionarItensAsync(itens);

        foreach (var item in salvos)
            if (!os.Itens.Contains(item)) os.Itens.Add(item);

        os.RecalcularTotal();
        await _repository.AtualizarTotalAsync(request.OrdemServicoId, os.Total);

        await _marcarAguardando.ExecutarAsync(new MarcarAguardandoAprovacaoRequest(request.OrdemServicoId, "sistema"));
        await _notificacao.EnviarOrcamentoAsync(request.OrdemServicoId, os.Cliente?.Email ?? string.Empty, os.Total);

        var response = salvos.Select(_mapper.MapToItemResponse);
        return Result<IEnumerable<OrdemServicoItemResponse>>.Success(response);
    }
}
```

> **Nota:** Este use case chama `IMarcarAguardandoAprovacaoUseCase` (Task 9). Na Task 19, a notificação por e-mail vira evento de domínio e essa chamada direta sai.

- [ ] **Step 6: `RemoverItemOSUseCase`**

`src/OficinaMecanica.Application/DTOs/Requests/RemoverItemOSRequest.cs`:
```csharp
namespace OficinaMecanica.Application.DTOs.Requests;

public record RemoverItemOSRequest(Guid OrdemServicoId, Guid ItemId);
```

`src/OficinaMecanica.Application/UseCases/OrdemServico/RemoverItemOS/IRemoverItemOSUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;

namespace OficinaMecanica.Application.UseCases.OrdemServico.RemoverItemOS;

public interface IRemoverItemOSUseCase : IUseCase<RemoverItemOSRequest, bool> { }
```

`src/OficinaMecanica.Application/UseCases/OrdemServico/RemoverItemOS/RemoverItemOSUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.RemoverItemOS;

public class RemoverItemOSUseCase : IRemoverItemOSUseCase
{
    private readonly IOrdemServicoRepository _repository;

    public RemoverItemOSUseCase(IOrdemServicoRepository repository) => _repository = repository;

    public async Task<Result<bool>> ExecutarAsync(RemoverItemOSRequest request)
    {
        var os = await _repository.ObterPorIdComItensAsync(request.OrdemServicoId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        if (!os.Itens.Any(i => i.Id == request.ItemId))
            return Result<bool>.NotFound("Item não encontrado nesta ordem de serviço.");

        await _repository.RemoverItemAsync(request.OrdemServicoId, request.ItemId);
        os.Itens = os.Itens.Where(i => i.Id != request.ItemId).ToList();
        os.RecalcularTotal();
        await _repository.AtualizarTotalAsync(request.OrdemServicoId, os.Total);

        return Result<bool>.Success(true);
    }
}
```

- [ ] **Step 7: `ObterTempoMedioExecucaoUseCase`**

`src/OficinaMecanica.Application/UseCases/OrdemServico/ObterTempoMedioExecucao/IObterTempoMedioExecucaoUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ObterTempoMedioExecucao;

public interface IObterTempoMedioExecucaoUseCase : IUseCase<Unit, double> { }
```

`src/OficinaMecanica.Application/UseCases/OrdemServico/ObterTempoMedioExecucao/ObterTempoMedioExecucaoUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ObterTempoMedioExecucao;

public class ObterTempoMedioExecucaoUseCase : IObterTempoMedioExecucaoUseCase
{
    private readonly IOrdemServicoRepository _repository;

    public ObterTempoMedioExecucaoUseCase(IOrdemServicoRepository repository) => _repository = repository;

    public async Task<Result<double>> ExecutarAsync(Unit _)
    {
        var horas = await _repository.GetTempoMedioExecucaoHorasAsync();
        return Result<double>.Success(horas);
    }
}
```

- [ ] **Step 8: Commit**

```bash
git add src/OficinaMecanica.Application/UseCases/OrdemServico/ \
        src/OficinaMecanica.Application/DTOs/Requests/AdicionarItensOSRequest.cs \
        src/OficinaMecanica.Application/DTOs/Requests/RemoverItemOSRequest.cs \
        src/OficinaMecanica.Application/Common/Unit.cs \
        tests/OficinaMecanica.Tests.Unit/UseCases/OrdemServico/
git commit -m "feat: cria use cases de OrdemServico (violação 8 parte 1/7)"
```

---

## Task 9: Use cases de OrdemServicoStatus

**Use cases a criar:**
1. `IniciarDiagnosticoUseCase`
2. `MarcarAguardandoAprovacaoUseCase`
3. `AprovarOSUseCase`
4. `RejeitarOSUseCase`
5. `NotificarConclusaoUseCase`
6. `EntregarOSUseCase`
7. `ForcarStatusOSUseCase`
8. `ObterHistoricoOSUseCase`

> Cada um deles segue o padrão: recebe `(Guid OsId, string AlteradoPor)` ou variantes, busca a OS via repositório, chama o método de domínio (`os.Aprovar`, etc.), persiste com `UpdateAsync`, retorna `Result<bool>`.

Para cada use case crie um Request específico em `Application/UseCases/OrdemServicoStatus/{Nome}/`.

- [ ] **Step 1: Padrão genérico — `AprovarOSUseCase`**

`src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/AprovarOS/AprovarOSRequest.cs`:
```csharp
namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOS;

public record AprovarOSRequest(Guid OsId, string AlteradoPor);
```

`src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/AprovarOS/IAprovarOSUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOS;

public interface IAprovarOSUseCase : IUseCase<AprovarOSRequest, bool> { }
```

`src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/AprovarOS/AprovarOSUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOS;

public class AprovarOSUseCase : IAprovarOSUseCase
{
    private readonly IOrdemServicoRepository _repository;

    public AprovarOSUseCase(IOrdemServicoRepository repository) => _repository = repository;

    public async Task<Result<bool>> ExecutarAsync(AprovarOSRequest request)
    {
        var os = await _repository.ObterPorIdAsync(request.OsId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        try
        {
            os.Aprovar(request.AlteradoPor);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Validation(ex.Message);
        }

        await _repository.UpdateAsync(os);
        return Result<bool>.Success(true);
    }
}
```

> **Nota:** O `try/catch` aqui captura validações de invariante do domínio (transição inválida de status) e converte para `Result.Validation`. Não é controle de fluxo, é tradução de erro de domínio para Result na borda do use case.

- [ ] **Step 2: `IniciarDiagnosticoUseCase`**

`src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/IniciarDiagnostico/IniciarDiagnosticoRequest.cs`:
```csharp
namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.IniciarDiagnostico;
public record IniciarDiagnosticoRequest(Guid OsId, string AlteradoPor);
```

Interface e classe seguem o padrão do Step 1, trocando `os.Aprovar` por `os.IniciarDiagnostico`.

- [ ] **Step 3: `MarcarAguardandoAprovacaoUseCase`**

`src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/MarcarAguardandoAprovacao/MarcarAguardandoAprovacaoRequest.cs`:
```csharp
namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;
public record MarcarAguardandoAprovacaoRequest(Guid OsId, string AlteradoPor);
```

Interface e classe: chama `os.EnviarParaAprovacao(request.AlteradoPor)`.

- [ ] **Step 4: `RejeitarOSUseCase`**

`src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/RejeitarOS/RejeitarOSUseCaseRequest.cs`:
```csharp
namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.RejeitarOS;
public record RejeitarOSUseCaseRequest(Guid OsId, string AlteradoPor, string Motivo);
```

Classe: chama `os.Rejeitar(request.AlteradoPor, request.Motivo)`.

- [ ] **Step 5: `NotificarConclusaoUseCase`**

`src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/NotificarConclusao/NotificarConclusaoRequest.cs`:
```csharp
namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.NotificarConclusao;
public record NotificarConclusaoRequest(Guid OsId, string AlteradoPor);
```

`NotificarConclusaoUseCase` — inclui chamada a `INotificacaoService` e log de warning via `IAppLogger` caso falhe:

```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.NotificarConclusao;

public class NotificarConclusaoUseCase : INotificarConclusaoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly INotificacaoService _notificacao;
    private readonly IAppLogger<NotificarConclusaoUseCase> _logger;

    public NotificarConclusaoUseCase(
        IOrdemServicoRepository repository,
        INotificacaoService notificacao,
        IAppLogger<NotificarConclusaoUseCase> logger)
    {
        _repository = repository;
        _notificacao = notificacao;
        _logger = logger;
    }

    public async Task<Result<bool>> ExecutarAsync(NotificarConclusaoRequest request)
    {
        var os = await _repository.ObterPorIdAsync(request.OsId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        try { os.Finalizar(request.AlteradoPor); }
        catch (InvalidOperationException ex) { return Result<bool>.Validation(ex.Message); }

        try
        {
            await _notificacao.EnviarConclusaoAsync(request.OsId, os.Cliente?.Email ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.Warning("Falha ao enviar notificação de conclusão para OS {OsId}.", ex, request.OsId);
        }

        await _repository.UpdateAsync(os);
        return Result<bool>.Success(true);
    }
}
```

- [ ] **Step 6: `EntregarOSUseCase`**

Request: `(Guid OsId, string AlteradoPor)`. Classe chama `os.Entregar(request.AlteradoPor)`.

- [ ] **Step 7: `ForcarStatusOSUseCase`**

`src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/ForcarStatusOS/ForcarStatusOSRequest.cs`:
```csharp
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.ForcarStatusOS;

public record ForcarStatusOSRequest(Guid OsId, EnumStatusOS NovoStatus, string AlteradoPor, string Motivo);
```

Classe chama `os.ForcarStatus(request.NovoStatus, request.AlteradoPor, request.Motivo)`.

- [ ] **Step 8: `ObterHistoricoOSUseCase`**

`src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/ObterHistoricoOS/IObterHistoricoOSUseCase.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.ObterHistoricoOS;

public interface IObterHistoricoOSUseCase : IUseCase<Guid, IEnumerable<HistoricoStatusOSResponse>> { }
```

```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.ObterHistoricoOS;

public class ObterHistoricoOSUseCase : IObterHistoricoOSUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly HistoricoStatusOSMapper _mapper;

    public ObterHistoricoOSUseCase(IOrdemServicoRepository repository, HistoricoStatusOSMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<HistoricoStatusOSResponse>>> ExecutarAsync(Guid osId)
    {
        var os = await _repository.ObterPorIdComHistoricoAsync(osId);
        if (os is null)
            return Result<IEnumerable<HistoricoStatusOSResponse>>.NotFound("Ordem de serviço não encontrada.");

        var resposta = os.Historico.OrderBy(h => h.AlteradoEm).Select(_mapper.MapToResponse);
        return Result<IEnumerable<HistoricoStatusOSResponse>>.Success(resposta);
    }
}
```

- [ ] **Step 9: Commit**

```bash
git add src/OficinaMecanica.Application/UseCases/OrdemServicoStatus/
git commit -m "feat: cria use cases de OrdemServicoStatus (violação 8 parte 2/7)"
```

---

## Task 10: Use cases de Cliente

**Use cases:** `CriarCliente`, `AtualizarCliente`, `ConsultarCliente` (por id), `ConsultarClientePorDocumento`, `ListarClientes`, `AtivarCliente`, `DesativarCliente`, `RemoverCliente`.

Padrão idêntico à Task 8. Cada use case em `Application/UseCases/Cliente/{NomeDoCaso}/`.

- [ ] **Step 1: `CriarClienteUseCase`**

```csharp
// Application/UseCases/Cliente/CriarCliente/ICriarClienteUseCase.cs
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Cliente.CriarCliente;

public interface ICriarClienteUseCase : IUseCase<CriarClienteRequest, ClienteResponse> { }
```

```csharp
// Application/UseCases/Cliente/CriarCliente/CriarClienteUseCase.cs
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Cliente.CriarCliente;

public class CriarClienteUseCase : ICriarClienteUseCase
{
    private readonly IClienteRepository _repository;
    private readonly ClienteMapper _mapper;

    public CriarClienteUseCase(IClienteRepository repository, ClienteMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ClienteResponse>> ExecutarAsync(CriarClienteRequest request)
    {
        if (await _repository.ExistsByDocumentoAsync(request.Documento))
            return Result<ClienteResponse>.Conflict("Cliente com este documento já cadastrado.");

        try
        {
            var cliente = new Domain.Entities.Cliente(request.Nome, request.Documento, request.Telefone, request.Email);
            var criado = await _repository.AddAsync(cliente);
            return Result<ClienteResponse>.Success(_mapper.MapToResponse(criado));
        }
        catch (ArgumentException ex)
        {
            return Result<ClienteResponse>.Validation(ex.Message);
        }
    }
}
```

- [ ] **Step 2: `AtualizarClienteUseCase`**

```csharp
// Request
public record AtualizarClienteUseCaseRequest(Guid Id, string Nome, string Telefone, string Email);
```

```csharp
public async Task<Result<ClienteResponse>> ExecutarAsync(AtualizarClienteUseCaseRequest request)
{
    var cliente = await _repository.GetByIdAsync(request.Id);
    if (cliente is null) return Result<ClienteResponse>.NotFound("Cliente não encontrado.");

    try { cliente.Atualizar(request.Nome, request.Telefone, request.Email); }
    catch (ArgumentException ex) { return Result<ClienteResponse>.Validation(ex.Message); }

    await _repository.UpdateAsync(cliente);
    return Result<ClienteResponse>.Success(_mapper.MapToResponse(cliente));
}
```

- [ ] **Step 3: `ConsultarClienteUseCase` (por id)**

```csharp
public interface IConsultarClienteUseCase : IUseCase<Guid, ClienteResponse> { }

public async Task<Result<ClienteResponse>> ExecutarAsync(Guid id)
{
    var cliente = await _repository.GetByIdAsync(id);
    return cliente is null
        ? Result<ClienteResponse>.NotFound("Cliente não encontrado.")
        : Result<ClienteResponse>.Success(_mapper.MapToResponse(cliente));
}
```

- [ ] **Step 4: `ConsultarClientePorDocumentoUseCase`**

```csharp
public interface IConsultarClientePorDocumentoUseCase : IUseCase<string, ClienteResponse> { }

public async Task<Result<ClienteResponse>> ExecutarAsync(string documento)
{
    var cliente = await _repository.GetByDocumentoAsync(documento);
    return cliente is null
        ? Result<ClienteResponse>.NotFound("Cliente não encontrado.")
        : Result<ClienteResponse>.Success(_mapper.MapToResponse(cliente));
}
```

- [ ] **Step 5: `ListarClientesUseCase`**

```csharp
public interface IListarClientesUseCase : IUseCase<Unit, IEnumerable<ClienteResponse>> { }

public async Task<Result<IEnumerable<ClienteResponse>>> ExecutarAsync(Unit _)
{
    var clientes = await _repository.GetAllAsync();
    return Result<IEnumerable<ClienteResponse>>.Success(clientes.Select(_mapper.MapToResponse));
}
```

- [ ] **Step 6: `AtivarClienteUseCase` e `DesativarClienteUseCase`**

Ambos recebem `Guid id`, buscam, mudam estado e persistem:
```csharp
public async Task<Result<bool>> ExecutarAsync(Guid id)
{
    var cliente = await _repository.GetByIdAsync(id);
    if (cliente is null) return Result<bool>.NotFound("Cliente não encontrado.");
    cliente.Ativar(); // ou Desativar()
    await _repository.UpdateAsync(cliente);
    return Result<bool>.Success(true);
}
```

- [ ] **Step 7: `RemoverClienteUseCase`**

```csharp
public interface IRemoverClienteUseCase : IUseCase<Guid, bool> { }

public async Task<Result<bool>> ExecutarAsync(Guid id)
{
    await _repository.DeleteAsync(id);
    return Result<bool>.Success(true);
}
```

- [ ] **Step 8: Commit**

```bash
git add src/OficinaMecanica.Application/UseCases/Cliente/
git commit -m "feat: cria use cases de Cliente (violação 8 parte 3/7)"
```

---

## Task 11: Use cases de Auth

**Use cases:** `AutenticarUsuario`, `RegistrarUsuario`.

- [ ] **Step 1: `AutenticarUsuarioUseCase`**

```csharp
// Application/UseCases/Auth/AutenticarUsuario/IAutenticarUsuarioUseCase.cs
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Auth.AutenticarUsuario;

public interface IAutenticarUsuarioUseCase : IUseCase<LoginRequest, TokenResponse> { }
```

```csharp
// Application/UseCases/Auth/AutenticarUsuario/AutenticarUsuarioUseCase.cs
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Auth.AutenticarUsuario;

public class AutenticarUsuarioUseCase : IAutenticarUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokenGenerator;

    public AutenticarUsuarioUseCase(
        IUsuarioRepository repository,
        IPasswordHasher hasher,
        ITokenGenerator tokenGenerator)
    {
        _repository = repository;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<TokenResponse>> ExecutarAsync(LoginRequest request)
    {
        var usuario = await _repository.ObterPorEmailAsync(request.Email.ToLower().Trim());
        if (usuario is null)
            return Result<TokenResponse>.Unauthorized("Credenciais inválidas.");

        if (!_hasher.Verificar(request.Senha, usuario.SenhaHash))
            return Result<TokenResponse>.Unauthorized("Credenciais inválidas.");

        return Result<TokenResponse>.Success(_tokenGenerator.GerarParaUsuario(usuario));
    }
}
```

> **Nota:** Note que `ITokenGenerator.GerarParaUsuario` retorna `TokenDto` (criado na Task 2). Renomeie para `TokenResponse` na assinatura agora que os DTOs foram reorganizados na Task 6.

Revise `src/OficinaMecanica.Application/Interfaces/ITokenGenerator.cs` e troque `TokenDto` por `TokenResponse`, e atualize `src/OficinaMecanica.Infrastructure/Auth/JwtTokenGenerator.cs` para retornar `TokenResponse`.

- [ ] **Step 2: `RegistrarUsuarioUseCase`**

```csharp
// Application/UseCases/Auth/RegistrarUsuario/IRegistrarUsuarioUseCase.cs
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.UseCases.Auth.RegistrarUsuario;

public interface IRegistrarUsuarioUseCase : IUseCase<RegistrarUsuarioRequest, Guid> { }
```

```csharp
// Application/UseCases/Auth/RegistrarUsuario/RegistrarUsuarioUseCase.cs
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Auth.RegistrarUsuario;

public class RegistrarUsuarioUseCase : IRegistrarUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _hasher;

    public RegistrarUsuarioUseCase(IUsuarioRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public async Task<Result<Guid>> ExecutarAsync(RegistrarUsuarioRequest request)
    {
        var existente = await _repository.ObterPorEmailAsync(request.Email.ToLower().Trim());
        if (existente is not null)
            return Result<Guid>.Conflict("Email já cadastrado.");

        var hash = _hasher.Hash(request.Senha);
        var usuario = new Usuario(request.Email, hash, request.Perfil);
        await _repository.AdicionarAsync(usuario);
        return Result<Guid>.Success(usuario.Id);
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add src/OficinaMecanica.Application/UseCases/Auth/ \
        src/OficinaMecanica.Application/Interfaces/ITokenGenerator.cs \
        src/OficinaMecanica.Infrastructure/Auth/JwtTokenGenerator.cs
git commit -m "feat: cria use cases de Auth e ajusta ITokenGenerator (violação 8 parte 4/7)"
```

---

## Task 12: Use cases de Veiculo

**Use cases:** `CriarVeiculo`, `AtualizarVeiculo`, `ConsultarVeiculo`, `ConsultarVeiculoPorPlaca`, `ListarVeiculos`, `ListarVeiculosPorCliente`, `RemoverVeiculo`.

Padrão idêntico à Task 10. Pontos específicos:

- [ ] **Step 1: `CriarVeiculoUseCase`**

```csharp
public async Task<Result<VeiculoResponse>> ExecutarAsync(CriarVeiculoRequest request)
{
    var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
    if (cliente is null)
        return Result<VeiculoResponse>.NotFound($"Cliente com ID {request.ClienteId} não encontrado.");

    if (await _veiculoRepository.ExistsByPlacaAsync(request.Placa))
        return Result<VeiculoResponse>.Conflict($"Veículo com placa {request.Placa} já cadastrado.");

    try
    {
        var veiculo = new Veiculo(request.ClienteId, request.Placa, request.Marca, request.Modelo, request.Ano);
        var criado = await _veiculoRepository.AddAsync(veiculo);
        return Result<VeiculoResponse>.Success(_mapper.MapToResponse(criado));
    }
    catch (ArgumentException ex)
    {
        return Result<VeiculoResponse>.Validation(ex.Message);
    }
}
```

- [ ] **Step 2: `AtualizarVeiculoUseCase`**

Request: `(Guid Id, Guid? ClienteId, string Placa, string Marca, string Modelo, int Ano)`.

Lógica:
- Buscar veículo, retornar `NotFound` se nulo
- Se ClienteId informado, validar existência
- Validar placa única em outro veículo (`ExistsByPlacaForOtherVeiculoAsync`)
- Chamar `veiculo.Atualizar(...)`, persistir, mapear

- [ ] **Step 3: Casos restantes (ConsultarVeiculo, ConsultarVeiculoPorPlaca, ListarVeiculos, ListarVeiculosPorCliente, RemoverVeiculo)**

Seguem o padrão Cliente (Task 10). Cada um recebe parâmetros simples e delega ao repositório.

- [ ] **Step 4: Commit**

```bash
git add src/OficinaMecanica.Application/UseCases/Veiculo/
git commit -m "feat: cria use cases de Veiculo (violação 8 parte 5/7)"
```

---

## Task 13: Use cases de Peca

**Use cases:** `CriarPeca`, `AtualizarPeca`, `ConsultarPeca`, `ListarPecas`, `ListarPecasPorNome`, `ListarPecasEstoqueBaixo`, `RemoverPeca`, `ObterEstoque`, `AtualizarEstoque`.

- [ ] **Step 1: `CriarPecaUseCase`**

```csharp
public async Task<Result<PecaResponse>> ExecutarAsync(CriarPecaRequest request)
{
    if (await _repository.ExistsByCodigoAsync(request.Codigo))
        return Result<PecaResponse>.Conflict("Já existe uma peça com este código.");

    try
    {
        var peca = new PecaInsumo(
            request.Nome, request.Codigo, request.Descricao,
            request.PrecoUnitario, request.Estoque);
        var criada = await _repository.AddAsync(peca);
        return Result<PecaResponse>.Success(_mapper.MapToResponse(criada));
    }
    catch (ArgumentException ex)
    {
        return Result<PecaResponse>.Validation(ex.Message);
    }
}
```

- [ ] **Step 2: `AtualizarEstoqueUseCase`**

Request: `(Guid Id, int Quantidade, string TipoOperacao)`.

```csharp
public async Task<Result<PecaResponse>> ExecutarAsync(AtualizarEstoqueUseCaseRequest request)
{
    PecaInsumo peca;
    if (request.TipoOperacao == "incrementar")
        peca = await _repository.IncrementarEstoqueAsync(request.Id, request.Quantidade);
    else if (request.TipoOperacao == "decrementar")
        peca = await _repository.DecrementarEstoqueAsync(request.Id, Math.Abs(request.Quantidade));
    else
        return Result<PecaResponse>.Validation(
            $"tipoOperacao inválido: '{request.TipoOperacao}'. Use 'incrementar' ou 'decrementar'.");

    return Result<PecaResponse>.Success(_mapper.MapToResponse(peca));
}
```

- [ ] **Step 3: Restantes**

Mesmo padrão de Cliente (Task 10).

- [ ] **Step 4: Commit**

```bash
git add src/OficinaMecanica.Application/UseCases/Peca/
git commit -m "feat: cria use cases de Peca (violação 8 parte 6/7)"
```

---

## Task 14: Use cases de Servico

**Use cases:** `CriarServico`, `AtualizarServico`, `ConsultarServico`, `ListarServicos`, `ListarServicosAtivos`, `ListarServicosPorNome`, `AtivarServico`, `DesativarServico`, `RemoverServico`.

Mesmo padrão de Cliente (Task 10).

- [ ] **Step 1: `CriarServicoUseCase`**

```csharp
public async Task<Result<ServicoResponse>> ExecutarAsync(CriarServicoRequest request)
{
    if (await _repository.ExistsByNomeAsync(request.Nome))
        return Result<ServicoResponse>.Conflict("Serviço com este nome já cadastrado.");

    try
    {
        var servico = new Servico(request.Nome, request.Descricao, request.Valor);
        var criado = await _repository.AddAsync(servico);
        return Result<ServicoResponse>.Success(_mapper.MapToResponse(criado));
    }
    catch (ArgumentException ex)
    {
        return Result<ServicoResponse>.Validation(ex.Message);
    }
}
```

- [ ] **Step 2-9: Restantes**

Seguem o padrão Cliente.

- [ ] **Step 10: Commit**

```bash
git add src/OficinaMecanica.Application/UseCases/Servico/
git commit -m "feat: cria use cases de Servico (violação 8 parte 7/7)"
```

---

## Task 15: Refatorar controllers e remover services antigos

**Objetivo:** Atualizar controllers para injetarem interfaces de use case, usar `MapError` em vez de try/catch, e deletar todos os `*Service.cs` e interfaces `I*Service.cs` (exceto `INotificacaoService` que permanece).

**Files:**
- Modify: todos os controllers em `src/OficinaMecanica.API/Controllers/`
- Delete: todos os arquivos `src/OficinaMecanica.Application/Services/*.cs` (já restam apenas os que sobraram após Tasks 2-4)
- Delete: `src/OficinaMecanica.Application/Interfaces/I{Cliente,OrdemServico,OrdemServicoStatus,Peca,Servico,Veiculo,Usuario}Service.cs`
- Modify: `src/OficinaMecanica.API/Program.cs` (registrar todos os use cases)
- Create: `src/OficinaMecanica.API/Common/ControllerExtensions.cs` (helper `MapError`)

- [ ] **Step 1: Helper de mapeamento de Result para IActionResult**

`src/OficinaMecanica.API/Common/ControllerExtensions.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.API.Common;

public static class ControllerExtensions
{
    public static IActionResult MapError<T>(this ControllerBase controller, Result<T> result) =>
        result.ErrorType switch
        {
            ResultErrorType.Validation   => controller.BadRequest(new { message = result.Error }),
            ResultErrorType.NotFound     => controller.NotFound(new { message = result.Error }),
            ResultErrorType.Conflict     => controller.Conflict(new { message = result.Error }),
            ResultErrorType.Unauthorized => controller.Unauthorized(new { message = result.Error }),
            _                            => controller.StatusCode(500, new { message = "Erro inesperado." })
        };
}
```

- [ ] **Step 2: Refatorar `OrdemServicosController`**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;
using OficinaMecanica.Application.UseCases.OrdemServico.AdicionarItensOS;
using OficinaMecanica.Application.UseCases.OrdemServico.ConsultarOrdemServico;
using OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;
using OficinaMecanica.Application.UseCases.OrdemServico.ObterTempoMedioExecucao;
using OficinaMecanica.Application.UseCases.OrdemServico.RemoverItemOS;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/ordens-servico")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class OrdemServicosController : ControllerBase
{
    private readonly IAbrirOrdemServicoUseCase _abrir;
    private readonly IConsultarOrdemServicoUseCase _consultar;
    private readonly IListarOrdensServicoUseCase _listar;
    private readonly IAdicionarItensOSUseCase _adicionarItens;
    private readonly IRemoverItemOSUseCase _removerItem;
    private readonly IObterTempoMedioExecucaoUseCase _tempoMedio;

    public OrdemServicosController(
        IAbrirOrdemServicoUseCase abrir,
        IConsultarOrdemServicoUseCase consultar,
        IListarOrdensServicoUseCase listar,
        IAdicionarItensOSUseCase adicionarItens,
        IRemoverItemOSUseCase removerItem,
        IObterTempoMedioExecucaoUseCase tempoMedio)
    {
        _abrir = abrir;
        _consultar = consultar;
        _listar = listar;
        _adicionarItens = adicionarItens;
        _removerItem = removerItem;
        _tempoMedio = tempoMedio;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoResumoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listar.ExecutarAsync(Unit.Value);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _consultar.ExecutarAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AbrirOrdemServicoRequest request)
    {
        var result = await _abrir.ExecutarAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : this.MapError(result);
    }

    [HttpPost("{id:guid}/itens")]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] List<AdicionarOSItemRequest> itens)
    {
        var request = new AdicionarItensOSRequest { OrdemServicoId = id, Itens = itens };
        var result = await _adicionarItens.ExecutarAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : this.MapError(result);
    }

    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        var result = await _removerItem.ExecutarAsync(new RemoverItemOSRequest(id, itemId));
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    [HttpGet("tempo-medio-execucao")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTempoMedioExecucao()
    {
        var result = await _tempoMedio.ExecutarAsync(Unit.Value);
        return result.IsSuccess
            ? Ok(new { tempoMedioHoras = result.Value })
            : this.MapError(result);
    }
}
```

- [ ] **Step 3: Refatorar `OrdemServicoStatusController`**

Padrão idêntico. Injetar `IIniciarDiagnosticoUseCase`, `IAprovarOSUseCase`, etc. Cada endpoint chama `ExecutarAsync` com o request adequado e retorna `NoContent()` no sucesso ou `this.MapError(result)`.

- [ ] **Step 4: Refatorar `ClientesController`, `AuthController`, `VeiculosController`, `PecasController`, `ServicosController`**

Mesmo padrão.

Para `AuthController`:
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var result = await _autenticar.ExecutarAsync(request);
    return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
}

[HttpPost("registrar")]
public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest request)
{
    var result = await _registrar.ExecutarAsync(request);
    return result.IsSuccess
        ? CreatedAtAction(nameof(Registrar), new { id = result.Value }, new { id = result.Value })
        : this.MapError(result);
}
```

- [ ] **Step 5: Atualizar `Program.cs` com todos os use cases**

Em `src/OficinaMecanica.API/Program.cs`, remover registros de `*Service` e adicionar registros de use cases. Exemplo (extrato):

```csharp
using OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;
using OficinaMecanica.Application.UseCases.OrdemServico.AdicionarItensOS;
using OficinaMecanica.Application.UseCases.OrdemServico.ConsultarOrdemServico;
using OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;
using OficinaMecanica.Application.UseCases.OrdemServico.ObterTempoMedioExecucao;
using OficinaMecanica.Application.UseCases.OrdemServico.RemoverItemOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.IniciarDiagnostico;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.NotificarConclusao;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.EntregarOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.RejeitarOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.ForcarStatusOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.ObterHistoricoOS;
using OficinaMecanica.Application.UseCases.Cliente.CriarCliente;
// ... demais use cases de Cliente, Auth, Veiculo, Peca, Servico

// OrdemServico
builder.Services.AddScoped<IAbrirOrdemServicoUseCase, AbrirOrdemServicoUseCase>();
builder.Services.AddScoped<IConsultarOrdemServicoUseCase, ConsultarOrdemServicoUseCase>();
builder.Services.AddScoped<IListarOrdensServicoUseCase, ListarOrdensServicoUseCase>();
builder.Services.AddScoped<IAdicionarItensOSUseCase, AdicionarItensOSUseCase>();
builder.Services.AddScoped<IRemoverItemOSUseCase, RemoverItemOSUseCase>();
builder.Services.AddScoped<IObterTempoMedioExecucaoUseCase, ObterTempoMedioExecucaoUseCase>();

// OrdemServicoStatus
builder.Services.AddScoped<IAprovarOSUseCase, AprovarOSUseCase>();
builder.Services.AddScoped<IIniciarDiagnosticoUseCase, IniciarDiagnosticoUseCase>();
builder.Services.AddScoped<IMarcarAguardandoAprovacaoUseCase, MarcarAguardandoAprovacaoUseCase>();
builder.Services.AddScoped<INotificarConclusaoUseCase, NotificarConclusaoUseCase>();
builder.Services.AddScoped<IEntregarOSUseCase, EntregarOSUseCase>();
builder.Services.AddScoped<IRejeitarOSUseCase, RejeitarOSUseCase>();
builder.Services.AddScoped<IForcarStatusOSUseCase, ForcarStatusOSUseCase>();
builder.Services.AddScoped<IObterHistoricoOSUseCase, ObterHistoricoOSUseCase>();

// Cliente — replicar para Auth, Veiculo, Peca, Servico
builder.Services.AddScoped<ICriarClienteUseCase, CriarClienteUseCase>();
// ...
```

Remover:
```csharp
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IServicoService, ServicoService>();
builder.Services.AddScoped<IPecaService, PecaService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IOrdemServicoService, OrdemServicoService>();
builder.Services.AddScoped<IOrdemServicoStatusService, OrdemServicoStatusService>();
```

- [ ] **Step 6: Deletar services e interfaces antigas**

```bash
cd /home/viniciusanjos/development/pessoal/oficina-mecanica
rm src/OficinaMecanica.Application/Services/ClienteService.cs
rm src/OficinaMecanica.Application/Services/OrdemServicoService.cs
rm src/OficinaMecanica.Application/Services/OrdemServicoStatusService.cs
rm src/OficinaMecanica.Application/Services/PecaService.cs
rm src/OficinaMecanica.Application/Services/ServicoService.cs
rm src/OficinaMecanica.Application/Services/UsuarioService.cs
rm src/OficinaMecanica.Application/Services/VeiculoService.cs
rmdir src/OficinaMecanica.Application/Services
rm src/OficinaMecanica.Application/Interfaces/IClienteService.cs
rm src/OficinaMecanica.Application/Interfaces/IOrdemServicoService.cs
rm src/OficinaMecanica.Application/Interfaces/IOrdemServicoStatusService.cs
rm src/OficinaMecanica.Application/Interfaces/IPecaService.cs
rm src/OficinaMecanica.Application/Interfaces/IServicoService.cs
rm src/OficinaMecanica.Application/Interfaces/IUsuarioService.cs
rm src/OficinaMecanica.Application/Interfaces/IVeiculoService.cs
```

- [ ] **Step 7: Deletar testes antigos de services**

```bash
rm tests/OficinaMecanica.Tests.Unit/Services/ClienteServiceTests.cs
rm tests/OficinaMecanica.Tests.Unit/Services/OrdemServicoServiceTests.cs
```

(Os testes de use case já existem por entidade após Tasks 8-14. Garanta cobertura mínima.)

- [ ] **Step 8: Build, testes de integração, commit**

```bash
dotnet build
dotnet test
git add -A
git commit -m "refactor: controllers usam use cases e MapError; remove services antigos"
```

---

# FASE 4 — Domain enhancements (Violações 7, 9)

## Task 16: Value Objects Email e Documento

**Files:**
- Create: `src/OficinaMecanica.Domain/ValueObjects/Email.cs`
- Create: `src/OficinaMecanica.Domain/ValueObjects/Documento.cs`
- Create: `src/OficinaMecanica.Domain/ValueObjects/TipoDocumento.cs`
- Test: `tests/OficinaMecanica.Tests.Unit/ValueObjects/EmailTests.cs`
- Test: `tests/OficinaMecanica.Tests.Unit/ValueObjects/DocumentoTests.cs`

- [ ] **Step 1: Testes de `Email`**

`tests/OficinaMecanica.Tests.Unit/ValueObjects/EmailTests.cs`:
```csharp
using FluentAssertions;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("a@b.com")]
    [InlineData("USER@DOMAIN.CO")]
    [InlineData(" mixed@case.com ")]
    public void Construtor_ComEmailValido_NormalizaParaLowerTrim(string entrada)
    {
        var email = new Email(entrada);
        email.Valor.Should().Be(entrada.Trim().ToLower());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba.com")]
    [InlineData("@sem-local.com")]
    [InlineData("falta-dominio@")]
    public void Construtor_ComEmailInvalido_LancaArgumentException(string entrada)
    {
        Action act = () => _ = new Email(entrada);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Records_ComMesmoValor_SaoIguais()
    {
        new Email("x@y.com").Should().Be(new Email("x@y.com"));
    }
}
```

- [ ] **Step 2: Rodar testes — falham (Email não existe)**

```bash
dotnet test --filter "FullyQualifiedName~EmailTests"
```

- [ ] **Step 3: Implementar `Email`**

`src/OficinaMecanica.Domain/ValueObjects/Email.cs`:
```csharp
using System.Text.RegularExpressions;

namespace OficinaMecanica.Domain.ValueObjects;

public sealed record Email
{
    public string Valor { get; }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) ||
            !Regex.IsMatch(valor.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Email inválido.", nameof(valor));
        Valor = valor.Trim().ToLower();
    }

    public override string ToString() => Valor;
    public static implicit operator string(Email e) => e.Valor;
}
```

- [ ] **Step 4: Rodar testes — passam**

- [ ] **Step 5: Testes de `Documento`**

`tests/OficinaMecanica.Tests.Unit/ValueObjects/DocumentoTests.cs`:
```csharp
using FluentAssertions;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.ValueObjects;

public class DocumentoTests
{
    [Fact]
    public void Construtor_CpfValido_DefineTipoCpf()
    {
        var doc = new Documento("123.456.789-09");
        doc.Tipo.Should().Be(TipoDocumento.Cpf);
        doc.Valor.Should().Be("12345678909");
    }

    [Fact]
    public void Construtor_CnpjValido_DefineTipoCnpj()
    {
        var doc = new Documento("11.222.333/0001-81");
        doc.Tipo.Should().Be(TipoDocumento.Cnpj);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("11111111111")]
    [InlineData("123")]
    public void Construtor_DocumentoInvalido_LancaArgumentException(string entrada)
    {
        Action act = () => _ = new Documento(entrada);
        act.Should().Throw<ArgumentException>();
    }
}
```

- [ ] **Step 6: Implementar `Documento` e `TipoDocumento`**

`src/OficinaMecanica.Domain/ValueObjects/TipoDocumento.cs`:
```csharp
namespace OficinaMecanica.Domain.ValueObjects;

public enum TipoDocumento { Cpf, Cnpj }
```

`src/OficinaMecanica.Domain/ValueObjects/Documento.cs`:
```csharp
using System.Text;

namespace OficinaMecanica.Domain.ValueObjects;

public sealed record Documento
{
    public string Valor { get; }
    public TipoDocumento Tipo { get; }

    public Documento(string valor)
    {
        var limpo = Limpar(valor ?? string.Empty);
        if (limpo.Length == 11 && ValidarCpf(limpo))
        {
            Tipo = TipoDocumento.Cpf;
            Valor = limpo;
            return;
        }
        if (limpo.Length == 14 && ValidarCnpj(limpo))
        {
            Tipo = TipoDocumento.Cnpj;
            Valor = limpo;
            return;
        }
        throw new ArgumentException("CPF ou CNPJ inválido.", nameof(valor));
    }

    public override string ToString() => Valor;
    public static implicit operator string(Documento d) => d.Valor;

    private static string Limpar(string s) =>
        new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpper();

    private static bool ValidarCpf(string cpf)
    {
        if (cpf.Distinct().Count() == 1) return false;
        int[] m1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var tmp = cpf.Substring(0, 9);
        var soma = tmp.Select((c, i) => int.Parse(c.ToString()) * m1[i]).Sum();
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        tmp += d1;
        soma = tmp.Select((c, i) => int.Parse(c.ToString()) * m2[i]).Sum();
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return cpf.EndsWith($"{d1}{d2}");
    }

    private static bool ValidarCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false;
        if (cnpj.Any(char.IsLetter))
            return ValidarCnpjAlfanumerico(cnpj);
        return cnpj.All(char.IsDigit) && ValidarCnpjNumerico(cnpj);
    }

    private static bool ValidarCnpjNumerico(string cnpj)
    {
        int[] m1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var tmp = cnpj.Substring(0, 12);
        var soma = tmp.Select((c, i) => int.Parse(c.ToString()) * m1[i]).Sum();
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        tmp += d1;
        soma = tmp.Select((c, i) => int.Parse(c.ToString()) * m2[i]).Sum();
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return cnpj.EndsWith($"{d1}{d2}");
    }

    private static bool ValidarCnpjAlfanumerico(string cnpj)
    {
        var sb = new StringBuilder();
        foreach (var c in cnpj)
        {
            if (char.IsDigit(c)) sb.Append(c);
            else if (char.IsLetter(c))
            {
                int v = char.ToUpper(c) - 'A';
                if (v < 0 || v > 9) return false;
                sb.Append(v);
            }
            else return false;
        }
        return ValidarCnpjNumerico(sb.ToString());
    }
}
```

- [ ] **Step 7: Rodar testes — passam**

- [ ] **Step 8: Commit**

```bash
git add src/OficinaMecanica.Domain/ValueObjects/ tests/OficinaMecanica.Tests.Unit/ValueObjects/
git commit -m "feat: cria Value Objects Email e Documento (violação 7 parte 1/2)"
```

---

## Task 17: Atualizar Cliente para usar VOs + ValueConverter no EF Core

**Files:**
- Modify: `src/OficinaMecanica.Domain/Entities/Cliente.cs`
- Modify: `src/OficinaMecanica.Infrastructure/Data/ApplicationDbContext.cs`
- Modify: `src/OficinaMecanica.Application/Mappers/ClienteMapper.cs`
- Modify: `src/OficinaMecanica.Application/UseCases/Cliente/CriarCliente/CriarClienteUseCase.cs` e use cases dependentes
- Modify: testes existentes que instanciam `Cliente` com strings

- [ ] **Step 1: Refatorar `Cliente` para usar VOs**

`src/OficinaMecanica.Domain/Entities/Cliente.cs`:
```csharp
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public Documento Documento { get; private set; }
    public string Telefone { get; private set; }
    public Email Email { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public bool Ativo { get; private set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();

    public Cliente(string nome, Documento documento, string telefone, Email email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório");
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone é obrigatório");

        Id = Guid.NewGuid();
        Nome = nome;
        Documento = documento;
        Telefone = telefone;
        Email = email;
        CriadoEm = DateTime.UtcNow;
        Ativo = true;
    }

    // Construtor parameterless privado para EF Core
    private Cliente() { Nome = string.Empty; Telefone = string.Empty; Documento = null!; Email = null!; }

    public void Atualizar(string nome, string telefone, Email email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório");
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone é obrigatório");

        Nome = nome;
        Telefone = telefone;
        Email = email;
    }

    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;
}
```

> Toda a lógica estática de validação de CPF/CNPJ e Email foi removida. Agora vive nos VOs `Documento` e `Email`.

- [ ] **Step 2: Atualizar `ApplicationDbContext` com ValueConverters**

Leia o arquivo atual e adicione, dentro de `OnModelCreating`, na configuração de `Cliente`:

```csharp
modelBuilder.Entity<Cliente>(b =>
{
    // ... config existente

    b.Property(c => c.Email)
        .HasConversion(
            v => v.Valor,
            v => new Email(v))
        .HasColumnName("email")
        .IsRequired();

    b.Property(c => c.Documento)
        .HasConversion(
            v => v.Valor,
            v => new Documento(v))
        .HasColumnName("documento")
        .IsRequired();
});
```

- [ ] **Step 3: Atualizar `ClienteMapper`**

Os campos `Email` e `Documento` agora são VOs com conversão implícita para string, mas é melhor explicitar:

```csharp
public ClienteResponse MapToResponse(Cliente cliente) => new()
{
    Id = cliente.Id,
    Nome = cliente.Nome,
    Documento = cliente.Documento.Valor,
    Telefone = cliente.Telefone,
    Email = cliente.Email.Valor,
    Ativo = cliente.Ativo,
    CriadoEm = cliente.CriadoEm
};
```

- [ ] **Step 4: Atualizar `CriarClienteUseCase` e `AtualizarClienteUseCase`**

```csharp
// CriarClienteUseCase.ExecutarAsync
try
{
    var documento = new Documento(request.Documento);
    var email = new Email(request.Email);
    var cliente = new Domain.Entities.Cliente(request.Nome, documento, request.Telefone, email);
    var criado = await _repository.AddAsync(cliente);
    return Result<ClienteResponse>.Success(_mapper.MapToResponse(criado));
}
catch (ArgumentException ex)
{
    return Result<ClienteResponse>.Validation(ex.Message);
}
```

```csharp
// AtualizarClienteUseCase
try
{
    var email = new Email(request.Email);
    cliente.Atualizar(request.Nome, request.Telefone, email);
}
catch (ArgumentException ex) { return Result<ClienteResponse>.Validation(ex.Message); }
```

E também o `ConsultarClientePorDocumentoUseCase` precisa validar a entrada:

```csharp
public async Task<Result<ClienteResponse>> ExecutarAsync(string documento)
{
    Documento doc;
    try { doc = new Documento(documento); }
    catch (ArgumentException ex) { return Result<ClienteResponse>.Validation(ex.Message); }

    var cliente = await _repository.GetByDocumentoAsync(doc.Valor);
    return cliente is null
        ? Result<ClienteResponse>.NotFound("Cliente não encontrado.")
        : Result<ClienteResponse>.Success(_mapper.MapToResponse(cliente));
}
```

- [ ] **Step 5: Atualizar testes existentes**

Em `tests/OficinaMecanica.Tests.Unit/Entities/ClienteTests.cs` e qualquer outro que instancie `Cliente`, trocar:
```csharp
new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com")
```
por:
```csharp
new Cliente("João Silva", new Documento("12345678909"), "(11) 99999-9999", new Email("joao@email.com"))
```

Buscar todos os usos:
```bash
grep -rn 'new Cliente(' tests/ src/ --include="*.cs"
```

- [ ] **Step 6: Validar build e testes**

```bash
dotnet build
dotnet test
```

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: Cliente usa Email e Documento VOs com EF Core ValueConverters (violação 7 parte 2/2)"
```

---

## Task 18: Infraestrutura de Domain Events

**Files:**
- Create: `src/OficinaMecanica.Domain/Common/IDomainEvent.cs`
- Create: `src/OficinaMecanica.Domain/Common/Entity.cs`
- Create: `src/OficinaMecanica.Application/Common/IDomainEventDispatcher.cs`
- Create: `src/OficinaMecanica.Application/Common/IEventHandler.cs`
- Create: `src/OficinaMecanica.Infrastructure/Events/DomainEventDispatcher.cs`

- [ ] **Step 1: `IDomainEvent` e `Entity` no Domain**

`src/OficinaMecanica.Domain/Common/IDomainEvent.cs`:
```csharp
namespace OficinaMecanica.Domain.Common;

public interface IDomainEvent
{
    DateTime OcorridoEm { get; }
}
```

`src/OficinaMecanica.Domain/Common/Entity.cs`:
```csharp
namespace OficinaMecanica.Domain.Common;

public abstract class Entity
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();
    protected void RaiseEvent(IDomainEvent evt) => _events.Add(evt);
    public void ClearEvents() => _events.Clear();
}
```

- [ ] **Step 2: Interfaces na Application**

`src/OficinaMecanica.Application/Common/IDomainEventDispatcher.cs`:
```csharp
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.Common;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events);
}
```

`src/OficinaMecanica.Application/Common/IEventHandler.cs`:
```csharp
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.Common;

public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent evt);
}
```

- [ ] **Step 3: Implementação em Infrastructure**

`src/OficinaMecanica.Infrastructure/Events/DomainEventDispatcher.cs`:
```csharp
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _provider;

    public DomainEventDispatcher(IServiceProvider provider) => _provider = provider;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events)
    {
        foreach (var evt in events)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(evt.GetType());
            var handlers = _provider.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                if (handler is null) continue;
                var method = handlerType.GetMethod("HandleAsync");
                if (method is null) continue;
                var task = (Task)method.Invoke(handler, new object[] { evt })!;
                await task;
            }
        }
    }
}
```

- [ ] **Step 4: Adicionar pacote `Microsoft.Extensions.DependencyInjection.Abstractions` (se não estiver presente)**

Em `src/OficinaMecanica.Infrastructure/OficinaMecanica.Infrastructure.csproj`, conferir se `Microsoft.Extensions.DependencyInjection.Abstractions` está disponível (geralmente vem transitivamente do `Microsoft.EntityFrameworkCore`). Se necessário, adicionar:
```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.6" />
```

- [ ] **Step 5: Registrar no `Program.cs`**

Em `src/OficinaMecanica.API/Program.cs`:
```csharp
using OficinaMecanica.Infrastructure.Events;
using OficinaMecanica.Application.Common;
// ...
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
```

- [ ] **Step 6: Commit**

```bash
git add src/OficinaMecanica.Domain/Common/ \
        src/OficinaMecanica.Application/Common/IDomainEventDispatcher.cs \
        src/OficinaMecanica.Application/Common/IEventHandler.cs \
        src/OficinaMecanica.Infrastructure/Events/ \
        src/OficinaMecanica.API/Program.cs
git commit -m "feat: infraestrutura de Domain Events (violação 9 parte 1/2)"
```

---

## Task 19: OrcamentoEnviadoEvent + handler + refatoração do use case

**Files:**
- Create: `src/OficinaMecanica.Domain/Events/OrcamentoEnviadoEvent.cs`
- Create: `src/OficinaMecanica.Application/EventHandlers/EnviarEmailOrcamentoHandler.cs`
- Modify: `src/OficinaMecanica.Domain/Entities/OrdemServico.cs` (herdar `Entity`, levantar evento)
- Modify: `src/OficinaMecanica.Application/UseCases/OrdemServico/AdicionarItensOS/AdicionarItensOSUseCase.cs` (despachar evento, remover chamada direta a `INotificacaoService`)
- Modify: `src/OficinaMecanica.API/Program.cs` (registrar handler)

- [ ] **Step 1: Criar evento**

`src/OficinaMecanica.Domain/Events/OrcamentoEnviadoEvent.cs`:
```csharp
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Events;

public record OrcamentoEnviadoEvent(
    Guid OrdemServicoId,
    string EmailCliente,
    decimal Total,
    DateTime OcorridoEm) : IDomainEvent;
```

- [ ] **Step 2: Refatorar `OrdemServico` para herdar `Entity` e levantar evento**

Em `src/OficinaMecanica.Domain/Entities/OrdemServico.cs`:

1. Adicionar `using OficinaMecanica.Domain.Common;` e `using OficinaMecanica.Domain.Events;`
2. Trocar declaração da classe para `public class OrdemServico : Entity`
3. No método `EnviarParaAprovacao(string alteradoPor)`, após a transição, adicionar:
   ```csharp
   RaiseEvent(new OrcamentoEnviadoEvent(
       Id, Cliente?.Email ?? string.Empty, Total, DateTime.UtcNow));
   ```

- [ ] **Step 3: Criar handler**

`src/OficinaMecanica.Application/EventHandlers/EnviarEmailOrcamentoHandler.cs`:
```csharp
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Application.EventHandlers;

public class EnviarEmailOrcamentoHandler : IEventHandler<OrcamentoEnviadoEvent>
{
    private readonly INotificacaoService _notificacao;

    public EnviarEmailOrcamentoHandler(INotificacaoService notificacao) =>
        _notificacao = notificacao;

    public Task HandleAsync(OrcamentoEnviadoEvent evt) =>
        _notificacao.EnviarOrcamentoAsync(evt.OrdemServicoId, evt.EmailCliente, evt.Total);
}
```

- [ ] **Step 4: Refatorar `AdicionarItensOSUseCase`**

Remover dependência direta de `INotificacaoService`. Agora a notificação acontece pelo handler do evento, disparado dentro de `EnviarParaAprovacao` (chamado dentro do `MarcarAguardandoAprovacaoUseCase`).

Substitua o construtor e o método de execução por:

```csharp
public class AdicionarItensOSUseCase : IAdicionarItensOSUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;
    private readonly IMarcarAguardandoAprovacaoUseCase _marcarAguardando;
    private readonly IDomainEventDispatcher _dispatcher;

    public AdicionarItensOSUseCase(
        IOrdemServicoRepository repository,
        OrdemServicoMapper mapper,
        IMarcarAguardandoAprovacaoUseCase marcarAguardando,
        IDomainEventDispatcher dispatcher)
    {
        _repository = repository;
        _mapper = mapper;
        _marcarAguardando = marcarAguardando;
        _dispatcher = dispatcher;
    }

    public async Task<Result<IEnumerable<OrdemServicoItemResponse>>> ExecutarAsync(AdicionarItensOSRequest request)
    {
        var os = await _repository.ObterPorIdComItensAsync(request.OrdemServicoId);
        if (os is null)
            return Result<IEnumerable<OrdemServicoItemResponse>>.NotFound("Ordem de serviço não encontrada.");

        var itens = new List<OrdemServicoItem>();
        foreach (var dto in request.Itens)
        {
            if (!Enum.TryParse<TipoOSItem>(dto.Tipo, ignoreCase: true, out var tipo))
                return Result<IEnumerable<OrdemServicoItemResponse>>.Validation("Tipo inválido. Use: servico, peca ou insumo.");

            itens.Add(new OrdemServicoItem(
                request.OrdemServicoId, tipo, dto.ReferenciaId,
                dto.Descricao, dto.Quantidade, dto.PrecoUnitario));
        }

        var salvos = await _repository.AdicionarItensAsync(itens);
        foreach (var item in salvos)
            if (!os.Itens.Contains(item)) os.Itens.Add(item);

        os.RecalcularTotal();
        await _repository.AtualizarTotalAsync(request.OrdemServicoId, os.Total);

        // MarcarAguardandoAprovacao internamente chama os.EnviarParaAprovacao,
        // que levanta OrcamentoEnviadoEvent no agregado.
        var marcarResult = await _marcarAguardando.ExecutarAsync(
            new MarcarAguardandoAprovacaoRequest(request.OrdemServicoId, "sistema"));
        if (!marcarResult.IsSuccess)
            return Result<IEnumerable<OrdemServicoItemResponse>>.Validation(marcarResult.Error ?? "Falha na transição de status.");

        // Despachar eventos acumulados
        var osComEventos = await _repository.ObterPorIdAsync(request.OrdemServicoId);
        if (osComEventos is not null && osComEventos.DomainEvents.Any())
        {
            await _dispatcher.DispatchAsync(osComEventos.DomainEvents);
            osComEventos.ClearEvents();
        }

        return Result<IEnumerable<OrdemServicoItemResponse>>.Success(salvos.Select(_mapper.MapToItemResponse));
    }
}
```

> **Nota arquitetural:** Esta implementação despacha os eventos a partir do agregado obtido após a transição. Em um cenário ideal, o `MarcarAguardandoAprovacaoUseCase` retornaria o agregado e/ou despacharia os eventos antes de persistir definitivamente. Como melhoria evolutiva, considere centralizar o despacho em um `SaveChangesAsync` interceptado no `ApplicationDbContext`.

- [ ] **Step 5: Atualizar `MarcarAguardandoAprovacaoUseCase` para também despachar (alternativa)**

Para garantir que eventos levantados sejam despachados próximos da transação, refatore `MarcarAguardandoAprovacaoUseCase`:

```csharp
public class MarcarAguardandoAprovacaoUseCase : IMarcarAguardandoAprovacaoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly IDomainEventDispatcher _dispatcher;

    public MarcarAguardandoAprovacaoUseCase(
        IOrdemServicoRepository repository,
        IDomainEventDispatcher dispatcher)
    {
        _repository = repository;
        _dispatcher = dispatcher;
    }

    public async Task<Result<bool>> ExecutarAsync(MarcarAguardandoAprovacaoRequest request)
    {
        var os = await _repository.ObterPorIdAsync(request.OsId);
        if (os is null) return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        try { os.EnviarParaAprovacao(request.AlteradoPor); }
        catch (InvalidOperationException ex) { return Result<bool>.Validation(ex.Message); }

        await _repository.UpdateAsync(os);

        if (os.DomainEvents.Any())
        {
            await _dispatcher.DispatchAsync(os.DomainEvents);
            os.ClearEvents();
        }

        return Result<bool>.Success(true);
    }
}
```

E remova o segundo despacho do `AdicionarItensOSUseCase` (basta delegar a `MarcarAguardandoAprovacaoUseCase`).

- [ ] **Step 6: Registrar handler no `Program.cs`**

```csharp
using OficinaMecanica.Application.EventHandlers;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Events;
// ...
builder.Services.AddScoped<IEventHandler<OrcamentoEnviadoEvent>, EnviarEmailOrcamentoHandler>();
```

- [ ] **Step 7: Validar build e testes**

```bash
dotnet build
dotnet test
```

- [ ] **Step 8: Commit final**

```bash
git add -A
git commit -m "feat: OrcamentoEnviadoEvent disparado por OrdemServico, handler envia notificação (violação 9 parte 2/2)"
```

---

## Marcos de validação final

Após concluir todas as tasks, verifique:

- [ ] `src/OficinaMecanica.Application/OficinaMecanica.Application.csproj` não tem `Konscious.Argon2`, `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.IdentityModel.Tokens`, nem `System.IdentityModel.Tokens.Jwt`
- [ ] Não existe `src/OficinaMecanica.Application/Services/` (pasta deletada)
- [ ] Nenhum controller usa `try/catch` para erros de negócio
- [ ] `dotnet build` sem warnings
- [ ] `dotnet test` 100% verde

```bash
cd /home/viniciusanjos/development/pessoal/oficina-mecanica

# 1) Validar Application.csproj sem pacotes proibidos
grep -E "Konscious|Microsoft.IdentityModel|System.IdentityModel|Configuration.Abstractions" \
  src/OficinaMecanica.Application/OficinaMecanica.Application.csproj
# Esperado: nenhuma linha

# 2) Validar ausência de Services antigos
ls src/OficinaMecanica.Application/Services 2>/dev/null
# Esperado: erro "no such file"

# 3) Validar ausência de try/catch de negócio nos controllers
grep -rn "catch (ArgumentException\|catch (KeyNotFoundException\|catch (InvalidOperationException" \
  src/OficinaMecanica.API/Controllers/
# Esperado: nenhuma linha

# 4) Build e testes
dotnet build
dotnet test
```

---

## Referências

- Spec deste plano: `docs/superpowers/specs/2026-05-16-clean-architecture-refatoracao-design.md`
- Documento de violações: `/home/viniciusanjos/Documents/violacoes-clean-architecture.md`
