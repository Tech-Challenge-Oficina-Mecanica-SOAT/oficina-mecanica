using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.Veiculo.RemoverVeiculo;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Veiculo.RemoverVeiculo;

public class RemoverVeiculoUseCaseTests
{
    private readonly Mock<IVeiculoRepository> _repo = new();
    private readonly RemoverVeiculoUseCase _sut;

    public RemoverVeiculoUseCaseTests()
    {
        _sut = new RemoverVeiculoUseCase(_repo.Object);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoVeiculoNaoExiste_RetornaNotFound()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Domain.Entities.Veiculo?)null);

        var result = await _sut.ExecutarAsync(id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoVeiculoExiste_DeletaERetornaSucesso()
    {
        var id = Guid.NewGuid();
        var veiculo = new Domain.Entities.Veiculo(Guid.NewGuid(), "ABC1234", "Ford", "Ka", 2020);
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(veiculo);
        _repo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _sut.ExecutarAsync(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
