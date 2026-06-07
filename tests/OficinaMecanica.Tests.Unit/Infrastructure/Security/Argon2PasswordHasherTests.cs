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
        _sut.Verify("senha-correta", hash).Should().BeTrue();
    }

    [Fact]
    public void Verificar_RetornaFalseParaSenhaErrada()
    {
        var hash = _sut.Hash("senha-correta");
        _sut.Verify("senha-errada", hash).Should().BeFalse();
    }

    [Fact]
    public void Verificar_RetornaFalseParaHashMalformado()
    {
        _sut.Verify("qualquer", "lixo-invalido").Should().BeFalse();
    }

    [Theory]
    [InlineData("$argon2i$v=19$m=9216,t=4,p=1$salt$hash")]           // prefixo errado (argon2i)
    [InlineData("$argon2id$v=19$m=9216,t=4$salt$hash")]               // paramParts.Length != 3
    [InlineData("$argon2id$v=19$m=9216,t=4,p=1$###$hash")]            // salt base64 inválido
    [InlineData("$argon2id$v=19$m=9216,t=4,p=1$YWJjMTIz$###")]        // hash base64 inválido
    [InlineData("$argon2id$v=19$m=9216,t=4,p=1$YWJjMTIz")]            // apenas 4 segmentos (parts.Length != 5)
    [InlineData("$argon2id$v=19$m=9216,t=4,x=1$YWJjMTIz$YWJjMTIz")]  // chave desconhecida 'x'
    [InlineData("$argon2id$v=19$m=9216,t=4,p=0$YWJjMTIz$YWJjMTIz")]  // p=0 (valor inválido)
    [InlineData("$argon2id$v=19$m=0,t=4,p=1$YWJjMTIz$YWJjMTIz")]     // m=0 (valor inválido)
    [InlineData("")]                                                    // hash vazio
    public void Verify_RetornaFalseParaHashInvalido(string hashInvalido)
    {
        _sut.Verify("qualquer", hashInvalido).Should().BeFalse();
    }

    [Fact]
    public void Hash_SenhaVazia_GeraHashValido()
    {
        var hash = _sut.Hash(string.Empty);
        hash.Should().StartWith("$argon2id$");
    }

    [Fact]
    public void Verify_SenhaVaziaCorreta_RetornaTrue()
    {
        var hash = _sut.Hash(string.Empty);
        _sut.Verify(string.Empty, hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_SenhaVaziaIncorreta_RetornaFalse()
    {
        var hash = _sut.Hash("outra-senha");
        _sut.Verify(string.Empty, hash).Should().BeFalse();
    }
}
