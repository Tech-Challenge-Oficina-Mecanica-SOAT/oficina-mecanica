using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;

public class MarcarAguardandoAprovacaoUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly Mock<IOrdemServicoMetrics> _metrics = new();
    private readonly MarcarAguardandoAprovacaoUseCase _sut;

    public MarcarAguardandoAprovacaoUseCaseTests() =>
        _sut = new MarcarAguardandoAprovacaoUseCase(_repo.Object, _metrics.Object);

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.OrdemServico?)null);
        var result = await _sut.ExecutarAsync(new MarcarAguardandoAprovacaoRequest(Guid.NewGuid(), "user"));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_TransicaoInvalida_RetornaValidation()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        _repo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);
        var result = await _sut.ExecutarAsync(new MarcarAguardandoAprovacaoRequest(os.Id, "user"));
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_TransitaParaAguardandoAprovacao()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        os.ForcarStatus(EnumStatusOS.EmDiagnostico, "admin", "setup");
        _repo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(new MarcarAguardandoAprovacaoRequest(os.Id, "user"));

        result.IsSuccess.Should().BeTrue();
        os.StatusOS.Should().Be(EnumStatusOS.AguardandoAprovacao);
    }
}
