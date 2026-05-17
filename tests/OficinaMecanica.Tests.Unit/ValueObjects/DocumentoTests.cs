using FluentAssertions;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.ValueObjects;

public class DocumentoTests
{
    [Fact]
    public void Construtor_CpfValido_DefineTipoCpf()
    {
        var doc = new Documento("123.456.789-09");
        doc.Tipo.Should().Be(TipoDocumento.Cpf);
        doc.Valor.Should().Be("12345678909");
    }

    [Fact]
    public void Construtor_CnpjValido_DefineTipoCnpj()
    {
        var doc = new Documento("11.222.333/0001-81");
        doc.Tipo.Should().Be(TipoDocumento.Cnpj);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("11111111111")]
    [InlineData("123")]
    public void Construtor_DocumentoInvalido_LancaArgumentException(string entrada)
    {
        Action act = () => _ = new Documento(entrada);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("12.ABC.345/01DE-XX")]
    public void Construtor_CnpjAlfanumericoComDigitoInvalido_LancaArgumentException(string entrada)
    {
        Action act = () => _ = new Documento(entrada);
        act.Should().Throw<ArgumentException>();
    }
}
