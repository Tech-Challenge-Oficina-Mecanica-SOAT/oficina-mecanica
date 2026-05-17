using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculosPorCliente;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Veiculo.ListarVeiculosPorCliente;

public class ListarVeiculosPorClienteUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaLista()
    {
        var clienteId = Guid.NewGuid();
        var repo = new Mock<IVeiculoRepository>();
        repo.Setup(r => r.GetByClienteIdAsync(clienteId)).ReturnsAsync(new[]
        {
            new Domain.Entities.Veiculo(clienteId, "ABC1234", "Fiat", "Uno", 2020)
        });
        var sut = new ListarVeiculosPorClienteUseCase(repo.Object, new VeiculoMapper());

        var result = await sut.ExecutarAsync(clienteId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }
}
