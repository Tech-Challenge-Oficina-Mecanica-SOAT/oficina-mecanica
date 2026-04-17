using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Tests.Unit.Services;

public class JwtServiceTests
{
    private readonly JwtService _sut;

    public JwtServiceTests()
    {
        var configValues = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "mecanica-jwt-secret-key-minimo-32-chars!!",
            ["Jwt:Issuer"]    = "mecanica-api",
            ["Jwt:Audience"]  = "mecanica-cliente",
            ["Jwt:ExpiracaoMinutos"] = "5"
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _sut = new JwtService(config);
    }

    [Fact]
    public void GerarToken_RetornaTokenNaoVazio()
    {
        var usuario = new Usuario("test@oficina.com", "hash", Perfil.Admin);

        var resultado = _sut.GerarToken(usuario);

        Assert.NotNull(resultado.Token);
        Assert.NotEmpty(resultado.Token);
    }

    [Fact]
    public void GerarToken_ExpiracaoEm5Minutos()
    {
        var usuario = new Usuario("test@oficina.com", "hash", Perfil.Admin);
        var antes = DateTime.UtcNow;

        var resultado = _sut.GerarToken(usuario);

        Assert.True(resultado.Expiracao > antes.AddMinutes(4));
        Assert.True(resultado.Expiracao < antes.AddMinutes(6));
    }

    [Fact]
    public void GerarToken_ContemClaimEmail()
    {
        var usuario = new Usuario("claims@oficina.com", "hash", Perfil.Admin);

        var resultado = _sut.GerarToken(usuario);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Token);

        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email
                                      && c.Value == "claims@oficina.com");
    }

    [Fact]
    public void GerarToken_ContemClaimRole()
    {
        var usuario = new Usuario("role@oficina.com", "hash", Perfil.Admin);

        var resultado = _sut.GerarToken(usuario);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Token);

        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GerarToken_ContemClaimSub()
    {
        var usuario = new Usuario("sub@oficina.com", "hash", Perfil.Cliente);

        var resultado = _sut.GerarToken(usuario);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Token);

        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub
                                      && c.Value == usuario.Id.ToString());
    }

    [Fact]
    public void GerarToken_TokensDiferentesParaUsuariosDiferentes()
    {
        var u1 = new Usuario("u1@oficina.com", "hash", Perfil.Admin);
        var u2 = new Usuario("u2@oficina.com", "hash", Perfil.Cliente);

        var t1 = _sut.GerarToken(u1).Token;
        var t2 = _sut.GerarToken(u2).Token;

        Assert.NotEqual(t1, t2);
    }
}
