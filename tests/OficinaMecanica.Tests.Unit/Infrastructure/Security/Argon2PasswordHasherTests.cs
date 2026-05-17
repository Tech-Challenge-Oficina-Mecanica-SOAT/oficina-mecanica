using FluentAssertions;
using Moq;
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
