using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.ForcarStatusOS;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServicoStatus.ForcarStatusOS;

public class ForcarStatusOSUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly Mock<IOrdemServicoMetrics> _metrics = new();
    private readonly ForcarStatusOSUseCase _sut;

    public ForcarStatusOSUseCaseTests() => _sut = new ForcarStatusOSUseCase(_repo.Object, _metrics.Object);

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.OrdemServico?)null);
        var result = await _sut.ExecutarAsync(new ForcarStatusOSRequest(Guid.NewGuid(), EnumStatusOS.EmExecucao, "user", "motivo"));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_AlteraStatus()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        _repo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(new ForcarStatusOSRequest(os.Id, EnumStatusOS.EmExecucao, "admin", "override"));

        result.IsSuccess.Should().BeTrue();
        os.StatusOS.Should().Be(EnumStatusOS.EmExecucao);
    }
}
