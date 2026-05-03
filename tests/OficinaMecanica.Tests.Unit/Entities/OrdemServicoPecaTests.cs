using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.Entities;

public class OrdemServicoPecaTests
{
    [Fact]
    public void OrdemServicoPeca_Quantidade_DeveRetornarValorAtribuido()
    {
        var peca = new OrdemServicoPeca { Quantidade = 2 };
        Assert.Equal(2, peca.Quantidade);
    }

    [Fact]
    public void OrdemServicoPeca_OrdemServicoId_DeveSetarERetornar()
    {
        var id = Guid.NewGuid();
        var peca = new OrdemServicoPeca { OrdemServicoId = id };
        Assert.Equal(id, peca.OrdemServicoId);
    }

    [Fact]
    public void OrdemServicoPeca_PecaInsumoId_DeveSetarERetornar()
    {
        var id = Guid.NewGuid();
        var peca = new OrdemServicoPeca { PecaInsumoId = id };
        Assert.Equal(id, peca.PecaInsumoId);
    }

    [Fact]
    public void OrdemServicoPeca_NavigationOrdemServico_DeveSetarERetornar()
    {
        var os = new OrdemServico();
        var peca = new OrdemServicoPeca { OrdemServico = os };
        Assert.Equal(os, peca.OrdemServico);
    }

    [Fact]
    public void OrdemServicoPeca_NavigationPecaInsumo_DeveSetarERetornar()
    {
        var pecaInsumo = new PecaInsumo("Filtro", "COD-001", "Desc", 10m, 1);
        var peca = new OrdemServicoPeca { PecaInsumo = pecaInsumo };
        Assert.Equal(pecaInsumo, peca.PecaInsumo);
    }

    [Fact]
    public void OrdemServicoPeca_TodasPropriedades_DeveSetarCorretamente()
    {
        var osId = Guid.NewGuid();
        var pecaInsumoId = Guid.NewGuid();
        var os = new OrdemServico();
        var pecaInsumo = new PecaInsumo("Filtro", "COD-002", "Desc", 10m, 1);

        var item = new OrdemServicoPeca
        {
            OrdemServicoId = osId,
            OrdemServico = os,
            PecaInsumoId = pecaInsumoId,
            PecaInsumo = pecaInsumo,
            Quantidade = 5
        };

        Assert.Equal(osId, item.OrdemServicoId);
        Assert.Equal(os, item.OrdemServico);
        Assert.Equal(pecaInsumoId, item.PecaInsumoId);
        Assert.Equal(pecaInsumo, item.PecaInsumo);
        Assert.Equal(5, item.Quantidade);
    }
}
