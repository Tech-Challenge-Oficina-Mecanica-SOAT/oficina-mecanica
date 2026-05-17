using FluentAssertions;
using Moq;
using OficinaMecanica.Application.UseCases.Veiculo.RemoverVeiculo;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Veiculo.RemoverVeiculo;

public class RemoverVeiculoUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_ChamaDelete()
    {
        var repo = new Mock<IVeiculoRepository>();
        var sut = new RemoverVeiculoUseCase(repo.Object);
        var id = Guid.NewGuid();

        var result = await sut.ExecutarAsync(id);

        result.IsSuccess.Should().BeTrue();
        repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
