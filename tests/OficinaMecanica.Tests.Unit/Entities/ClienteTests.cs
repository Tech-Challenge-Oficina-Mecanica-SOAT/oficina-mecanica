using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.Entities;


public class ClienteTests
{
    [Fact]
    public void Construtor_DeveCriarCliente_QuandoDadosValidos()
    {
        var nome = "João da Silva";
        var documento = "123.456.789-09"; // CPF válido
        var telefone = "11999999999";
        var email = "joao@email.com";

        var cliente = new Cliente(nome, documento, telefone, email);

        Assert.Equal(nome, cliente.Nome);
        Assert.Equal(Cliente.ValidarDocumento(documento), true);
        Assert.Equal(Cliente.ValidarDocumento(cliente.Documento), true);
        Assert.Equal(telefone, cliente.Telefone);
        Assert.Equal(email.ToLower(), cliente.Email);
        Assert.True(cliente.Ativo);
        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.True((DateTime.UtcNow - cliente.CriadoEm).TotalSeconds < 5);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoNomeInvalido(string nome)
    {
        Assert.Throws<ArgumentException>(() =>
            new Cliente(nome, "12345678909", "11999999999", "teste@email.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("11111111111")]
    [InlineData("00000000000000")]
    [InlineData("1234567890123")]
    public void Construtor_DeveLancarExcecao_QuandoDocumentoInvalido(string documento)
    {
        Assert.Throws<ArgumentException>(() =>
            new Cliente("Nome", documento, "11999999999", "teste@email.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoTelefoneInvalido(string telefone)
    {
        Assert.Throws<ArgumentException>(() =>
            new Cliente("Nome", "12345678909", telefone, "teste@email.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("emailinvalido")]
    [InlineData("email@")]
    [InlineData("@email.com")]
    [InlineData("email@email")]
    public void Construtor_DeveLancarExcecao_QuandoEmailInvalido(string email)
    {
        Assert.Throws<ArgumentException>(() =>
            new Cliente("Nome", "12345678909", "11999999999", email));
    }

    [Fact]
    public void Atualizar_DeveAlterarDados_QuandoValidos()
    {
        var cliente = new Cliente("Nome", "12345678909", "11999999999", "teste@email.com");
        cliente.Atualizar("Novo Nome", "11888888888", "novo@email.com");

        Assert.Equal("Novo Nome", cliente.Nome);
        Assert.Equal("11888888888", cliente.Telefone);
        Assert.Equal("novo@email.com", cliente.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_DeveLancarExcecao_QuandoNomeInvalido(string nome)
    {
        var cliente = new Cliente("Nome", "12345678909", "11999999999", "teste@email.com");
        Assert.Throws<ArgumentException>(() =>
            cliente.Atualizar(nome, "11999999999", "teste@email.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_DeveLancarExcecao_QuandoTelefoneInvalido(string telefone)
    {
        var cliente = new Cliente("Nome", "12345678909", "11999999999", "teste@email.com");
        Assert.Throws<ArgumentException>(() =>
            cliente.Atualizar("Nome", telefone, "teste@email.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("emailinvalido")]
    [InlineData("email@")]
    [InlineData("@email.com")]
    [InlineData("email@email")]
    public void Atualizar_DeveLancarExcecao_QuandoEmailInvalido(string email)
    {
        var cliente = new Cliente("Nome", "12345678909", "11999999999", "teste@email.com");
        Assert.Throws<ArgumentException>(() =>
            cliente.Atualizar("Nome", "11999999999", email));
    }

    [Fact]
    public void Desativar_DeveDefinirAtivoComoFalse()
    {
        var cliente = new Cliente("Nome", "12345678909", "11999999999", "teste@email.com");
        cliente.Desativar();
        Assert.False(cliente.Ativo);
    }

    [Fact]
    public void Ativar_DeveDefinirAtivoComoTrue()
    {
        var cliente = new Cliente("Nome", "12345678909", "11999999999", "teste@email.com");
        cliente.Desativar();
        cliente.Ativar();
        Assert.True(cliente.Ativo);
    }

    [Theory]
    [InlineData("12345678909")] // CPF válido
    [InlineData("52998224725")] // CPF válido
    public void ValidarDocumento_DeveRetornarTrue_ParaCpfValido(string cpf)
    {
        Assert.True(Cliente.ValidarDocumento(cpf));
    }

    [Theory]
    [InlineData("12345678901")] // CPF inválido
    [InlineData("11111111111")] // CPF inválido
    public void ValidarDocumento_DeveRetornarFalse_ParaCpfInvalido(string cpf)
    {
        Assert.False(Cliente.ValidarDocumento(cpf));
    }

    [Theory]
    [InlineData("11222333000181")] // CNPJ válido
    [InlineData("19131243000197")] // CNPJ válido
    public void ValidarDocumento_DeveRetornarTrue_ParaCnpjValido(string cnpj)
    {
        Assert.True(Cliente.ValidarDocumento(cnpj));
    }

    [Theory]
    [InlineData("11222333000180")] // CNPJ inválido
    [InlineData("00000000000000")] // CNPJ inválido
    public void ValidarDocumento_DeveRetornarFalse_ParaCnpjInvalido(string cnpj)
    {
        Assert.False(Cliente.ValidarDocumento(cnpj));
    }

    [Theory]
    //[InlineData("A1222333000123")] // CNPJ alfanumérico válido (A=0)
    [InlineData("B1222333000181")] // CNPJ alfanumérico válido (B=1)
    public void ValidarDocumento_DeveRetornarTrue_ParaCnpjAlfanumericoValido(string cnpjAlfa)
    {
        Assert.True(Cliente.ValidarDocumento(cnpjAlfa));
    }

    [Theory]
    [InlineData("K1222333000181")] // Letra inválida (K=10)
    [InlineData("A1222333000180")] // Dígito verificador inválido
    public void ValidarDocumento_DeveRetornarFalse_ParaCnpjAlfanumericoInvalido(string cnpjAlfa)
    {
        Assert.False(Cliente.ValidarDocumento(cnpjAlfa));
    }

    [Theory]
    [InlineData("email@email.com")]
    [InlineData("teste.teste@dominio.com.br")]
    public void ValidarEmail_DeveRetornarTrue_ParaEmailValido(string email)
    {
        var metodo = typeof(Cliente).GetMethod("ValidarEmail", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Assert.True((bool)metodo.Invoke(null, new object[] { email }));
    }

    [Theory]
    [InlineData("emailinvalido")]
    [InlineData("email@")]
    [InlineData("@email.com")]
    [InlineData("email@email")]
    public void ValidarEmail_DeveRetornarFalse_ParaEmailInvalido(string email)
    {
        var metodo = typeof(Cliente).GetMethod("ValidarEmail", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Assert.False((bool)metodo.Invoke(null, new object[] { email }));
    }
}
