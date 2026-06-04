using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Peca.ListarPecasPorNome;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Peca.ListarPecasPorNome;

public class ListarPecasPorNomeUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaLista()
    {
        var repo = new Mock<IPecaInsumoRepository>();
        repo.Setup(r => r.GetByNomeAsync("F")).ReturnsAsync(new[] { new PecaInsumo("F", "FIL", "d", 1m, 1) });
        var sut = new ListarPecasPorNomeUseCase(repo.Object, new PecaMapper());

        var result = await sut.ExecutarAsync("F");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
