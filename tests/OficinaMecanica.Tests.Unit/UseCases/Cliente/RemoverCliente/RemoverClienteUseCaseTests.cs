using FluentAssertions;
using Moq;
using OficinaMecanica.Application.UseCases.Cliente.RemoverCliente;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Cliente.RemoverCliente;

public class RemoverClienteUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_ChamaDelete_RetornaSuccess()
    {
        var repo = new Mock<IClienteRepository>();
        var sut = new RemoverClienteUseCase(repo.Object);
        var id = Guid.NewGuid();

        var result = await sut.ExecutarAsync(id);

        result.IsSuccess.Should().BeTrue();
        repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
