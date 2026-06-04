using FluentAssertions;
using Moq;
using OficinaMecanica.Application.UseCases.OrdemServico.ObterTempoMedioExecucao;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServico.ObterTempoMedioExecucao;

public class ObterTempoMedioExecucaoUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaTempoMedio()
    {
        var repo = new Mock<IOrdemServicoRepository>();
        repo.Setup(r => r.GetTempoMedioExecucaoHorasAsync()).ReturnsAsync(12.5);
        var sut = new ObterTempoMedioExecucaoUseCase(repo.Object);

        var result = await sut.ExecutarAsync(default(OficinaMecanica.Application.Common.Unit));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(12.5);
    }
}
