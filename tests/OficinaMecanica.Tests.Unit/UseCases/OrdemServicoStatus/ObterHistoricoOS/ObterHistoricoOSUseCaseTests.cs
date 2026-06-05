using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.ObterHistoricoOS;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServicoStatus.ObterHistoricoOS;

public class ObterHistoricoOSUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repo = new();
    private readonly ObterHistoricoOSUseCase _sut;

    public ObterHistoricoOSUseCaseTests() => _sut = new ObterHistoricoOSUseCase(_repo.Object, new HistoricoStatusOSMapper());

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.ObterPorIdComHistoricoAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Entities.OrdemServico?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_OSEncontrada_RetornaHistorico()
    {
        var os = new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        _repo.Setup(r => r.ObterPorIdComHistoricoAsync(os.Id)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(os.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().NotBeEmpty();
    }
}
