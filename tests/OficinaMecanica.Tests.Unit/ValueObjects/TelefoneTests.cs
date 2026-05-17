using FluentAssertions;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.ValueObjects;

public class TelefoneTests
{
    [Theory]
    [InlineData("(11) 99999-9999", "11999999999")]
    [InlineData("11 99999-9999", "11999999999")]
    [InlineData("+55 11 99999-9999", "5511999999999")]
    [InlineData("1199999999", "1199999999")]
    public void Construtor_ComTelefoneValido_NormalizaParaDigitos(string entrada, string esperado)
    {
        var tel = new Telefone(entrada);
        tel.Valor.Should().Be(esperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("abcdefghij")]
    public void Construtor_ComTelefoneInvalido_LancaArgumentException(string entrada)
    {
        Action act = () => _ = new Telefone(entrada);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Records_ComMesmoValor_SaoIguais()
    {
        new Telefone("(11) 99999-9999").Should().Be(new Telefone("11999999999"));
    }
}
