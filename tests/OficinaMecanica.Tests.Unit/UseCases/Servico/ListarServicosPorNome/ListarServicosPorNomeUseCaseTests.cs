using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Servico.ListarServicosPorNome;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Servico.ListarServicosPorNome;

public class ListarServicosPorNomeUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaServicosFiltrados()
    {
        var repo = new Mock<IServicoRepository>();
        repo.Setup(r => r.GetByNomeAsync("Troca")).ReturnsAsync(new[] { new Domain.Entities.Servico("Troca", "d", 10m) });
        var sut = new ListarServicosPorNomeUseCase(repo.Object, new ServicoMapper());

        var result = await sut.ExecutarAsync("Troca");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
