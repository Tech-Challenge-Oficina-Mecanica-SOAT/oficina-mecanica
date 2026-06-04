using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.UseCases.Auth.RegistrarUsuario;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Auth.RegistrarUsuario;

public class RegistrarUsuarioUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly RegistrarUsuarioUseCase _sut;

    public RegistrarUsuarioUseCaseTests() => _sut = new RegistrarUsuarioUseCase(_repo.Object, _hasher.Object);

    [Fact]
    public async Task ExecutarAsync_EmailDuplicado_RetornaConflict()
    {
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new Usuario("a@b.com", "h", Perfil.Admin));

        var result = await _sut.ExecutarAsync(new RegistrarUsuarioRequest("a@b.com", "senha", Perfil.Admin));

        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_RegistraUsuario()
    {
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _hasher.Setup(h => h.Hash("senha")).Returns("hashed");

        var result = await _sut.ExecutarAsync(new RegistrarUsuarioRequest("a@b.com", "senha", Perfil.Admin));

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.AdicionarAsync(It.Is<Usuario>(u => u.SenhaHash == "hashed")), Times.Once);
    }
}
