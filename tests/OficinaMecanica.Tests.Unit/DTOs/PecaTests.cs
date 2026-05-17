using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Tests.Unit.DTOs;

public class PecaResponseTests
{
    [Fact]
    public void PecaDto_DeveInicializarComValoresPadrao()
    {
        var peca = new PecaResponse();

        Assert.Equal(Guid.Empty, peca.Id);
        Assert.Equal(string.Empty, peca.Nome);
        Assert.Equal(string.Empty, peca.Codigo);
        Assert.Equal(string.Empty, peca.Descricao);
        Assert.Equal(0m, peca.PrecoUnitario);
        Assert.Equal(0, peca.Estoque);
        Assert.False(peca.Ativo);
    }

    [Fact]
    public void PecaDto_DeveAtribuirValoresCorretamente()
    {
        var id = Guid.NewGuid();
        var criadoEm = DateTime.UtcNow;
        var atualizadoEm = DateTime.UtcNow.AddHours(1);

        var peca = new PecaResponse
        {
            Id = id,
            Nome = "Óleo Motor",
            Codigo = "OL-001",
            Descricao = "Óleo sintético 5W-30",
            PrecoUnitario = 45.50m,
            Estoque = 100,
            CriadoEm = criadoEm,
            AtualizadoEm = atualizadoEm,
            Ativo = true
        };

        Assert.Equal(id, peca.Id);
        Assert.Equal("Óleo Motor", peca.Nome);
        Assert.Equal("OL-001", peca.Codigo);
        Assert.Equal("Óleo sintético 5W-30", peca.Descricao);
        Assert.Equal(45.50m, peca.PrecoUnitario);
        Assert.Equal(100, peca.Estoque);
        Assert.Equal(criadoEm, peca.CriadoEm);
        Assert.Equal(atualizadoEm, peca.AtualizadoEm);
        Assert.True(peca.Ativo);
    }
}

public class CreatePecaResponseTests
{
    [Fact]
    public void CreatePecaDto_DeveInicializarComValoresPadrao()
    {
        var createPeca = new CriarPecaRequest();

        Assert.Equal(string.Empty, createPeca.Nome);
        Assert.Equal(string.Empty, createPeca.Codigo);
        Assert.Equal(string.Empty, createPeca.Descricao);
        Assert.Equal(0m, createPeca.PrecoUnitario);
        Assert.Equal(0, createPeca.Estoque);
    }

    [Fact]
    public void CreatePecaDto_DeveAtribuirValoresCorretamente()
    {
        var createPeca = new CriarPecaRequest
        {
            Nome = "Filtro Óleo",
            Codigo = "FO-001",
            Descricao = "Filtro de óleo original",
            PrecoUnitario = 28.90m,
            Estoque = 50
        };

        Assert.Equal("Filtro Óleo", createPeca.Nome);
        Assert.Equal("FO-001", createPeca.Codigo);
        Assert.Equal("Filtro de óleo original", createPeca.Descricao);
        Assert.Equal(28.90m, createPeca.PrecoUnitario);
        Assert.Equal(50, createPeca.Estoque);
    }
}

public class UpdatePecaResponseTests
{
    [Fact]
    public void UpdatePecaDto_DeveInicializarComValoresPadrao()
    {
        var updatePeca = new AtualizarPecaRequest();

        Assert.Equal(string.Empty, updatePeca.Nome);
        Assert.Equal(string.Empty, updatePeca.Descricao);
        Assert.Equal(0m, updatePeca.PrecoUnitario);
        Assert.Equal(0, updatePeca.Estoque);
    }

    [Fact]
    public void UpdatePecaDto_DeveAtribuirValoresCorretamente()
    {
        var updatePeca = new AtualizarPecaRequest
        {
            Nome = "Correia Serpentina",
            Descricao = "Correia de transmissão atualizada",
            PrecoUnitario = 85.00m,
            Estoque = 30
        };

        Assert.Equal("Correia Serpentina", updatePeca.Nome);
        Assert.Equal("Correia de transmissão atualizada", updatePeca.Descricao);
        Assert.Equal(85.00m, updatePeca.PrecoUnitario);
        Assert.Equal(30, updatePeca.Estoque);
    }
}

public class AtualizarEstoqueRequestTests
{
    [Fact]
    public void UpdateEstoqueDto_DeveInicializarComValoresPadrao()
    {
        var updateEstoque = new AtualizarEstoqueRequest();

        Assert.Equal(0, updateEstoque.Quantidade);
        Assert.Equal(string.Empty, updateEstoque.TipoOperacao);
    }

    [Fact]
    public void UpdateEstoqueDto_DeveAtribuirValoresCorretamente()
    {
        var updateEstoque = new AtualizarEstoqueRequest
        {
            Quantidade = 10,
            TipoOperacao = "Entrada"
        };

        Assert.Equal(10, updateEstoque.Quantidade);
        Assert.Equal("Entrada", updateEstoque.TipoOperacao);
    }

    [Theory]
    [InlineData(5, "Entrada")]
    [InlineData(3, "Saída")]
    [InlineData(1, "Ajuste")]
    public void UpdateEstoqueDto_DeveSuportarDiferentesTiposOperacao(int quantidade, string tipoOperacao)
    {
        var updateEstoque = new AtualizarEstoqueRequest
        {
            Quantidade = quantidade,
            TipoOperacao = tipoOperacao
        };

        Assert.Equal(quantidade, updateEstoque.Quantidade);
        Assert.Equal(tipoOperacao, updateEstoque.TipoOperacao);
    }
}