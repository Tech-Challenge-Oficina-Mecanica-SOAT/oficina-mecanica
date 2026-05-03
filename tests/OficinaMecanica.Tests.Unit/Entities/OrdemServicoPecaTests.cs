using Moq;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.Entities;

public class OrdemServicoPecaTests
{
    private readonly Mock<IOrdemServicoRepository> _repositoryMock;
    private readonly Mock<IOrdemServicoStatusService> _statusServiceMock;
    private readonly Mock<INotificacaoService> _notificacaoServiceMock;
    private readonly OrdemServicoService _service;

    public OrdemServicoPecaTests()
    {
        _repositoryMock = new Mock<IOrdemServicoRepository>();
        _statusServiceMock = new Mock<IOrdemServicoStatusService>();
        _notificacaoServiceMock = new Mock<INotificacaoService>();
        _service = new OrdemServicoService(
            _repositoryMock.Object,
            _statusServiceMock.Object,
            _notificacaoServiceMock.Object);
    }


    [Fact]
    public void OrdemServicoPeca_Getters_DeveRetornarValores()
    {
        var peca = new OrdemServicoPeca { OrdemServicoId = Guid.NewGuid(), Quantidade = 2 };
        Assert.Equal(2, peca.Quantidade);
    }

}
