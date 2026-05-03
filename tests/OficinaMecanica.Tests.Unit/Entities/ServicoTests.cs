using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.Entities;

public class ServicoTests
{
    [Fact]
    public void Construtor_ComParametrosValidos_DeveCriarServico()
    {
        // Arrange
        var nome = "Troca de Óleo";
        var descricao = "Troca de óleo do motor";
        var valor = 150.00m;

        // Act
        var servico = new Servico(nome, descricao, valor);

        // Assert
        Assert.NotEqual(Guid.Empty, servico.Id);
        Assert.Equal(nome, servico.Nome);
        Assert.Equal(descricao, servico.Descricao);
        Assert.Equal(valor, servico.Valor);
        Assert.True(servico.Ativo);
        Assert.NotEqual(DateTime.MinValue, servico.CriadoEm);
    }

    [Fact]
    public void Construtor_ComNomeVazio_DeveThrowArgumentException()
    {
        // Arrange
        var descricao = "Descrição";
        var valor = 150.00m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Servico(string.Empty, descricao, valor));
    }

    [Fact]
    public void Construtor_ComNomeNulo_DeveThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new Servico(null!, "Descrição", 150.00m));
    }

    [Fact]
    public void Construtor_ComValorZero_DeveThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new Servico("Serviço", "Descrição", 0));
    }

    [Fact]
    public void Construtor_ComValorNegativo_DeveThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new Servico("Serviço", "Descrição", -50));
    }

    [Fact]
    public void Construtor_ComDescricaoNula_DeveDefinirDescricaoVazia()
    {
        // Arrange
        var nome = "Serviço";
        var valor = 150.00m;

        // Act
        var servico = new Servico(nome, null!, valor);

        // Assert
        Assert.Equal(string.Empty, servico.Descricao);
    }

    [Fact]
    public void Atualizar_ComParametrosValidos_DeveAtualizarServico()
    {
        // Arrange
        var servico = new Servico("Nome Antigo", "Descrição Antiga", 100);
        var novoNome = "Nome Novo";
        var novaDescricao = "Descrição Nova";
        var novoValor = 200.00m;

        // Act
        servico.Atualizar(novoNome, novaDescricao, novoValor);

        // Assert
        Assert.Equal(novoNome, servico.Nome);
        Assert.Equal(novaDescricao, servico.Descricao);
        Assert.Equal(novoValor, servico.Valor);
    }

    [Fact]
    public void Atualizar_ComNomeVazio_DeveThrowArgumentException()
    {
        // Arrange
        var servico = new Servico("Nome", "Descrição", 100);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => servico.Atualizar(string.Empty, "Descrição", 150));
    }

    [Fact]
    public void Atualizar_ComValorInvalido_DeveThrowArgumentException()
    {
        // Arrange
        var servico = new Servico("Nome", "Descrição", 100);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => servico.Atualizar("Nome", "Descrição", 0));
    }

    [Fact]
    public void Desativar_DeveAlterarStatusParaFalso()
    {
        // Arrange
        var servico = new Servico("Serviço", "Descrição", 100);

        // Act
        servico.Desativar();

        // Assert
        Assert.False(servico.Ativo);
    }

    [Fact]
    public void Ativar_DeveAlterarStatusParaVerdadeiro()
    {
        // Arrange
        var servico = new Servico("Serviço", "Descrição", 100);
        servico.Desativar();

        // Act
        servico.Ativar();

        // Assert
        Assert.True(servico.Ativo);
    }

    [Fact]
    public void OrdensServico_DeveInicializarComListaVazia()
    {
        // Arrange & Act
        var servico = new Servico("Serviço", "Descrição", 100);

        // Assert
        Assert.NotNull(servico.OrdensServico);
        Assert.Empty(servico.OrdensServico);
    }
}