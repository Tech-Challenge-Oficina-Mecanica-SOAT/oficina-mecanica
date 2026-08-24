using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOS;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServicoStatus.AprovarOS;

public class AprovarOSUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly Mock<IOrdemServicoMetrics> _metrics = new();
    private readonly AprovarOSUseCase _sut;

    public AprovarOSUseCaseTests() => _sut = new AprovarOSUseCase(_repo.Object, _metrics.Object);

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.OrdemServico?)null);
        var result = await _sut.ExecutarAsync(new AprovarOSRequest(Guid.NewGuid(), "user"));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_StatusInvalido_RetornaValidation()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        _repo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);
        var result = await _sut.ExecutarAsync(new AprovarOSRequest(os.Id, "user"));
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_TransitaParaEmExecucao()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        os.ForcarStatus(EnumStatusOS.AguardandoAprovacao, "admin", "setup");
        _repo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(new AprovarOSRequest(os.Id, "user"));

        result.IsSuccess.Should().BeTrue();
        os.StatusOS.Should().Be(EnumStatusOS.EmExecucao);
    }
}
