using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.OrdemServico.ListarOrdensServico;

public class ListarOrdensServicoUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_ListaVazia_RetornaSuccessVazio()
    {
        var repo = new Mock<IOrdemServicoRepository>();
        repo.Setup(r => r.ListarTodosAsync()).ReturnsAsync(Array.Empty<Domain.Entities.OrdemServico>());
        var sut = new ListarOrdensServicoUseCase(repo.Object, new OrdemServicoMapper());

        var result = await sut.ExecutarAsync(default(OficinaMecanica.Application.Common.Unit));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecutarAsync_ComItens_RetornaLista()
    {
        var repo = new Mock<IOrdemServicoRepository>();
        repo.Setup(r => r.ListarTodosAsync()).ReturnsAsync(new[]
        {
            new Domain.Entities.OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "o")
        });
        var sut = new ListarOrdensServicoUseCase(repo.Object, new OrdemServicoMapper());

        var result = await sut.ExecutarAsync(default(OficinaMecanica.Application.Common.Unit));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
