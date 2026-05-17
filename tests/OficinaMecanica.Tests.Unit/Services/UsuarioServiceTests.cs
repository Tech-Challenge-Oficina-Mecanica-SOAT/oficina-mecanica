using Moq;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly UsuarioService _sut;
    private int _hashCounter;

    public UsuarioServiceTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _hasherMock = new Mock<IPasswordHasher>();

        _hasherMock.Setup(h => h.Hash(It.IsAny<string>()))
            .Returns<string>(senha =>
            {
                _hashCounter++;
                return $"hashed::{senha}::{_hashCounter}";
            });

        _hasherMock.Setup(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((senha, hash) => hash.StartsWith($"hashed::{senha}::"));

        _sut = new UsuarioService(_repositoryMock.Object, _hasherMock.Object);
    }

    [Fact]
    public async Task Autenticar_ComCredenciaisValidas_RetornaUsuario()
    {
        var dto = new RegistrarUsuarioDto("admin@oficina.com", "Senha@123", Perfil.Admin);
        var usuario = await _sut.RegistrarAsync(dto);

        _repositoryMock.Setup(r => r.ObterPorEmailAsync("admin@oficina.com"))
            .ReturnsAsync(usuario);

        var resultado = await _sut.AutenticarAsync("admin@oficina.com", "Senha@123");

        Assert.NotNull(resultado);
        Assert.Equal("admin@oficina.com", resultado.Email);
    }

    [Fact]
    public async Task Autenticar_ComSenhaErrada_RetornaNull()
    {
        var dto = new RegistrarUsuarioDto("user@oficina.com", "SenhaCorreta@1", Perfil.Admin);
        var usuario = await _sut.RegistrarAsync(dto);

        _repositoryMock.Setup(r => r.ObterPorEmailAsync("user@oficina.com"))
            .ReturnsAsync(usuario);

        var resultado = await _sut.AutenticarAsync("user@oficina.com", "SenhaErrada");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Autenticar_ComEmailInexistente_RetornaNull()
    {
        _repositoryMock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

        var resultado = await _sut.AutenticarAsync("naoexiste@oficina.com", "Senha@123");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Registrar_ComDadosValidos_CriaUsuarioComHashDiferente()
    {
        _repositoryMock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
            .Returns(Task.CompletedTask);

        var dto = new RegistrarUsuarioDto("novo@oficina.com", "Senha@123", Perfil.Admin);
        var usuario = await _sut.RegistrarAsync(dto);

        Assert.NotNull(usuario);
        Assert.Equal("novo@oficina.com", usuario.Email);
        Assert.NotEqual("Senha@123", usuario.SenhaHash);
        _hasherMock.Verify(h => h.Hash("Senha@123"), Times.Once);
    }

    [Fact]
    public async Task Registrar_MesmaSenha_GeraHashesDiferentes()
    {
        _repositoryMock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
            .Returns(Task.CompletedTask);

        var u1 = await _sut.RegistrarAsync(new RegistrarUsuarioDto("a@test.com", "Senha@123"));
        var u2 = await _sut.RegistrarAsync(new RegistrarUsuarioDto("b@test.com", "Senha@123"));

        Assert.NotEqual(u1.SenhaHash, u2.SenhaHash);
    }

    [Fact]
    public async Task Registrar_ComEmailDuplicado_LancaInvalidOperationException()
    {
        var usuarioExistente = new Usuario("dup@oficina.com", "hash", Perfil.Admin);
        _repositoryMock.Setup(r => r.ObterPorEmailAsync("dup@oficina.com"))
            .ReturnsAsync(usuarioExistente);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.RegistrarAsync(new RegistrarUsuarioDto("dup@oficina.com", "Senha@123")));
    }

    [Fact]
    public async Task Registrar_EmailNormalizado_SalvoEmMinusculas()
    {
        _repositoryMock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
            .Returns(Task.CompletedTask);

        var usuario = await _sut.RegistrarAsync(new RegistrarUsuarioDto("ADMIN@OFICINA.COM", "Senha@123"));

        Assert.Equal("admin@oficina.com", usuario.Email);
    }
}
