using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.UseCases.Auth.AutenticarUsuario;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Auth.AutenticarUsuario;

public class AutenticarUsuarioUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenGenerator> _token = new();
    private readonly AutenticarUsuarioUseCase _sut;

    public AutenticarUsuarioUseCaseTests() =>
        _sut = new AutenticarUsuarioUseCase(_repo.Object, _hasher.Object, _token.Object);

    [Fact]
    public async Task ExecutarAsync_UsuarioInexistente_RetornaUnauthorized()
    {
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        var result = await _sut.ExecutarAsync(new LoginRequest("a@b.com", "x"));
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
    }

    [Fact]
    public async Task ExecutarAsync_SenhaErrada_RetornaUnauthorized()
    {
        var u = new Usuario("a@b.com", "hash", Perfil.Admin);
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>())).ReturnsAsync(u);
        _hasher.Setup(h => h.Verify("x", "hash")).Returns(false);

        var result = await _sut.ExecutarAsync(new LoginRequest("a@b.com", "x"));

        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_RetornaToken()
    {
        var u = new Usuario("a@b.com", "hash", Perfil.Admin);
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>())).ReturnsAsync(u);
        _hasher.Setup(h => h.Verify("senha", "hash")).Returns(true);
        var tok = new TokenResponse("token", DateTime.UtcNow.AddHours(1));
        _token.Setup(t => t.GerarParaUsuario(u)).Returns(tok);

        var result = await _sut.ExecutarAsync(new LoginRequest("a@b.com", "senha"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("token");
    }
}
