using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.NotificarConclusao;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServicoStatus.NotificarConclusao;

public class NotificarConclusaoUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly NotificarConclusaoUseCase _sut;

    public NotificarConclusaoUseCaseTests() =>
        _sut = new NotificarConclusaoUseCase(_repo.Object);

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.OrdemServico?)null);
        var result = await _sut.ExecutarAsync(new NotificarConclusaoRequest(Guid.NewGuid(), "user"));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_FinalizaOS()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        os.ForcarStatus(EnumStatusOS.EmExecucao, "admin", "setup");
        _repo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(new NotificarConclusaoRequest(os.Id, "user"));

        result.IsSuccess.Should().BeTrue();
        os.StatusOS.Should().Be(EnumStatusOS.Finalizada);
    }
}
