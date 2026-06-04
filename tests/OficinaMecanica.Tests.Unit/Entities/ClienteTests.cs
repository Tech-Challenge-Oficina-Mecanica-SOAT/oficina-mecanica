using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.Entities;


public class ClienteTests
{
    private static Documento Doc(string s = "12345678909") => new(s);
    private static Email Mail(string s = "teste@email.com") => new(s);

    [Fact]
    public void Construtor_DeveCriarCliente_QuandoDadosValidos()
    {
        var nome = "João da Silva";
        var documento = new Documento("123.456.789-09"); // CPF válido
        var telefone = "11999999999";
        var email = new Email("joao@email.com");

        var cliente = new Cliente(nome, documento, telefone, email);

        Assert.Equal(nome, cliente.Nome);
        Assert.Equal("12345678909", cliente.Documento.Valor);
        Assert.Equal(telefone, cliente.Telefone);
        Assert.Equal("joao@email.com", cliente.Email.Valor);
        Assert.True(cliente.Ativo);
        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.True((DateTime.UtcNow - cliente.CriadoEm).TotalSeconds < 5);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoNomeInvalido(string? nome)
    {
        Assert.Throws<ArgumentException>(() =>
            new Cliente(nome!, Doc(), "11999999999", Mail()));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("11111111111")]
    [InlineData("00000000000000")]
    [InlineData("1234567890123")]
    public void Documento_DeveLancarExcecao_QuandoValorInvalido(string documento)
    {
        Assert.Throws<ArgumentException>(() => new Documento(documento));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoTelefoneInvalido(string? telefone)
    {
        Assert.Throws<ArgumentException>(() =>
            new Cliente("Nome", Doc(), telefone!, Mail()));
    }

    [Theory]
    [InlineData("emailinvalido")]
    [InlineData("email@")]
    [InlineData("@email.com")]
    [InlineData("email@email")]
    public void Email_DeveLancarExcecao_QuandoValorInvalido(string email)
    {
        Assert.Throws<ArgumentException>(() => new Email(email));
    }

    [Fact]
    public void Atualizar_DeveAlterarDados_QuandoValidos()
    {
        var cliente = new Cliente("Nome", Doc(), "11999999999", Mail());
        cliente.Atualizar("Novo Nome", "11888888888", new Email("novo@email.com"));

        Assert.Equal("Novo Nome", cliente.Nome);
        Assert.Equal("11888888888", cliente.Telefone);
        Assert.Equal("novo@email.com", cliente.Email.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_DeveLancarExcecao_QuandoNomeInvalido(string? nome)
    {
        var cliente = new Cliente("Nome", Doc(), "11999999999", Mail());
        Assert.Throws<ArgumentException>(() =>
            cliente.Atualizar(nome!, "11999999999", Mail()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_DeveLancarExcecao_QuandoTelefoneInvalido(string? telefone)
    {
        var cliente = new Cliente("Nome", Doc(), "11999999999", Mail());
        Assert.Throws<ArgumentException>(() =>
            cliente.Atualizar("Nome", telefone!, Mail()));
    }

    [Fact]
    public void Desativar_DeveDefinirAtivoComoFalse()
    {
        var cliente = new Cliente("Nome", Doc(), "11999999999", Mail());
        cliente.Desativar();
        Assert.False(cliente.Ativo);
    }

    [Fact]
    public void Ativar_DeveDefinirAtivoComoTrue()
    {
        var cliente = new Cliente("Nome", Doc(), "11999999999", Mail());
        cliente.Desativar();
        cliente.Ativar();
        Assert.True(cliente.Ativo);
    }

    [Theory]
    [InlineData("12345678909")] // CPF válido
    [InlineData("52998224725")] // CPF válido
    public void Documento_DeveCriar_ParaCpfValido(string cpf)
    {
        var d = new Documento(cpf);
        Assert.Equal(TipoDocumento.Cpf, d.Tipo);
    }

    [Theory]
    [InlineData("12345678901")] // CPF inválido
    [InlineData("11111111111")] // CPF inválido
    public void Documento_DeveLancar_ParaCpfInvalido(string cpf)
    {
        Assert.Throws<ArgumentException>(() => new Documento(cpf));
    }

    [Theory]
    [InlineData("11222333000181")] // CNPJ válido
    [InlineData("19131243000197")] // CNPJ válido
    public void Documento_DeveCriar_ParaCnpjValido(string cnpj)
    {
        var d = new Documento(cnpj);
        Assert.Equal(TipoDocumento.Cnpj, d.Tipo);
    }

    [Theory]
    [InlineData("11222333000180")] // CNPJ inválido
    [InlineData("00000000000000")] // CNPJ inválido
    public void Documento_DeveLancar_ParaCnpjInvalido(string cnpj)
    {
        Assert.Throws<ArgumentException>(() => new Documento(cnpj));
    }

    [Theory]
    [InlineData("B1222333000181")] // CNPJ alfanumérico válido (B=1)
    public void Documento_DeveCriar_ParaCnpjAlfanumericoValido(string cnpjAlfa)
    {
        var d = new Documento(cnpjAlfa);
        Assert.Equal(TipoDocumento.Cnpj, d.Tipo);
    }

    [Theory]
    [InlineData("K1222333000181")] // Letra inválida (K=10)
    [InlineData("A1222333000180")] // Dígito verificador inválido
    public void Documento_DeveLancar_ParaCnpjAlfanumericoInvalido(string cnpjAlfa)
    {
        Assert.Throws<ArgumentException>(() => new Documento(cnpjAlfa));
    }

    [Theory]
    [InlineData("email@email.com")]
    [InlineData("teste.teste@dominio.com.br")]
    public void Email_DeveCriar_ParaValorValido(string email)
    {
        var e = new Email(email);
        Assert.Equal(email.ToLower(), e.Valor);
    }
}
