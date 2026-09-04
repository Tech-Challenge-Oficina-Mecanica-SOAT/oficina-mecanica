using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.IniciarDiagnostico;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServicoStatus.IniciarDiagnostico;

public class IniciarDiagnosticoUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly Mock<IOrdemServicoMetrics> _metrics = new();
    private readonly IniciarDiagnosticoUseCase _sut;

    public IniciarDiagnosticoUseCaseTests() => _sut = new IniciarDiagnosticoUseCase(_repo.Object, _metrics.Object);

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.OrdemServico?)null);
        var result = await _sut.ExecutarAsync(new IniciarDiagnosticoRequest(Guid.NewGuid(), "user"));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_TransitaParaEmDiagnostico()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        _repo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(new IniciarDiagnosticoRequest(os.Id, "user"));

        result.IsSuccess.Should().BeTrue();
        os.StatusOS.Should().Be(EnumStatusOS.EmDiagnostico);
    }

    [Fact]
    public async Task ExecutarAsync_StatusInvalido_RetornaValidation()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        os.ForcarStatus(EnumStatusOS.EmExecucao, "admin", "setup");
        _repo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(new IniciarDiagnosticoRequest(os.Id, "user"));

        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }
}
