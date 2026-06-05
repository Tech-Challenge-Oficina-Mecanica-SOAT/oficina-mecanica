using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.UseCases.OrdemServico.RemoverItemOS;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServico.RemoverItemOS;

public class RemoverItemOSUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly RemoverItemOSUseCase _sut;

    public RemoverItemOSUseCaseTests() => _sut = new RemoverItemOSUseCase(_repo.Object);

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorIdComItensAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.OrdemServico?)null);
        var result = await _sut.ExecutarAsync(new RemoverItemOSRequest(Guid.NewGuid(), Guid.NewGuid()));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_ItemNaoEncontrado_RetornaNotFound()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        _repo.Setup(r => r.ObterPorIdComItensAsync(os.Id)).ReturnsAsync(os);
        var result = await _sut.ExecutarAsync(new RemoverItemOSRequest(os.Id, Guid.NewGuid()));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_RemoveItem()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        var item = new OrdemServicoItem(os.Id, TipoOSItem.Servico, Guid.NewGuid(), "x", 1, 10m);
        os.Itens.Add(item);
        _repo.Setup(r => r.ObterPorIdComItensAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(new RemoverItemOSRequest(os.Id, item.Id));

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.RemoverItemAsync(os.Id, item.Id), Times.Once);
    }
}
