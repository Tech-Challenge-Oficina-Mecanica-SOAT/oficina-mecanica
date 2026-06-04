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
