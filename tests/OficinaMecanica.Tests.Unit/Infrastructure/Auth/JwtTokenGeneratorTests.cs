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
