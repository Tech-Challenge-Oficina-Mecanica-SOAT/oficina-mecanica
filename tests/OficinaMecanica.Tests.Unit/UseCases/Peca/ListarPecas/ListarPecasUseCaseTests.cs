using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Peca.ListarPecas;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Peca.ListarPecas;

public class ListarPecasUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaLista()
    {
        var repo = new Mock<IPecaInsumoRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { new PecaInsumo("a", "b", "c", 1m, 1) });
        var sut = new ListarPecasUseCase(repo.Object, new PecaMapper());

        var result = await sut.ExecutarAsync(default(OficinaMecanica.Application.Common.Unit));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
