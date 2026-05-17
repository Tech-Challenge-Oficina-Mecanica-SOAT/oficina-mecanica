using FluentAssertions;
using Moq;
using OficinaMecanica.Application.UseCases.Servico.RemoverServico;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Servico.RemoverServico;

public class RemoverServicoUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_ChamaDelete()
    {
        var repo = new Mock<IServicoRepository>();
        var sut = new RemoverServicoUseCase(repo.Object);
        var id = Guid.NewGuid();

        var result = await sut.ExecutarAsync(id);

        result.IsSuccess.Should().BeTrue();
        repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
