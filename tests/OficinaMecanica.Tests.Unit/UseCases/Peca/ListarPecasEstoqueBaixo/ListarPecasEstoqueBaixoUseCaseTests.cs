using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Peca.ListarPecasEstoqueBaixo;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Peca.ListarPecasEstoqueBaixo;

public class ListarPecasEstoqueBaixoUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaLista()
    {
        var repo = new Mock<IPecaInsumoRepository>();
        repo.Setup(r => r.GetByEstoqueBaixoAsync(5)).ReturnsAsync(new[] { new PecaInsumo("F", "FIL", "d", 1m, 2) });
        var sut = new ListarPecasEstoqueBaixoUseCase(repo.Object, new PecaMapper());

        var result = await sut.ExecutarAsync(5);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
