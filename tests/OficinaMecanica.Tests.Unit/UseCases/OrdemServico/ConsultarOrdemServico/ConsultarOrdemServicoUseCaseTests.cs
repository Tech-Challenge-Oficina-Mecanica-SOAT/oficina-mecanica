using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.OrdemServico.ConsultarOrdemServico;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServico.ConsultarOrdemServico;

public class ConsultarOrdemServicoUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly ConsultarOrdemServicoUseCase _sut;

    public ConsultarOrdemServicoUseCaseTests() => _sut = new ConsultarOrdemServicoUseCase(_repo.Object, new OrdemServicoMapper());

    [Fact]
    public async Task ExecutarAsync_NaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorIdComItensAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.OrdemServico?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrada_RetornaSuccess()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        _repo.Setup(r => r.ObterPorIdComItensAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(os.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(os.Id);
    }
}
