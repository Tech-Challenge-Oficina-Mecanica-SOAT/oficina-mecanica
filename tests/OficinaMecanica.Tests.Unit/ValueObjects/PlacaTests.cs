using FluentAssertions;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.ValueObjects;

public class PlacaTests
{
    [Theory]
    [InlineData("ABC1234", "ABC1234")]
    [InlineData("abc1234", "ABC1234")]
    [InlineData("ABC-1234", "ABC1234")]
    [InlineData(" abc1234 ", "ABC1234")]
    public void Construtor_PlacaAntigaValida_NormalizaParaUppercase(string entrada, string esperado)
    {
        var placa = new Placa(entrada);
        placa.Valor.Should().Be(esperado);
    }

    [Theory]
    [InlineData("ABC1D23", "ABC1D23")]
    [InlineData("abc1d23", "ABC1D23")]
    public void Construtor_PlacaMercosulValida_NormalizaParaUppercase(string entrada, string esperado)
    {
        var placa = new Placa(entrada);
        placa.Valor.Should().Be(esperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABC")]
    [InlineData("123456789")]
    [InlineData("ABCD1234")]
    [InlineData("AB12345")]
    public void Construtor_PlacaInvalida_LancaArgumentException(string entrada)
    {
        Action act = () => _ = new Placa(entrada);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Records_ComMesmoValorNormalizado_SaoIguais()
    {
        new Placa("abc-1234").Should().Be(new Placa("ABC1234"));
    }
}
