using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculos;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Veiculo.ListarVeiculos;

public class ListarVeiculosUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaLista()
    {
        var repo = new Mock<IVeiculoRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new Domain.Entities.Veiculo(Guid.NewGuid(), "ABC1234", "Fiat", "Uno", 2020)
        });
        var sut = new ListarVeiculosUseCase(repo.Object, new VeiculoMapper());

        var result = await sut.ExecutarAsync(default(OficinaMecanica.Application.Common.Unit));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
