using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.Servico.RemoverServico;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Servico.RemoverServico;

public class RemoverServicoUseCaseTests
{
    private readonly Mock<IServicoRepository> _repo = new();
    private readonly RemoverServicoUseCase _sut;

    public RemoverServicoUseCaseTests()
    {
        _sut = new RemoverServicoUseCase(_repo.Object);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoServicoNaoExiste_RetornaNotFound()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Domain.Entities.Servico?)null);

        var result = await _sut.ExecutarAsync(id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoServicoExiste_DeletaERetornaSucesso()
    {
        var id = Guid.NewGuid();
        var servico = new Domain.Entities.Servico("Troca de óleo", "Óleo sintético 5W30", 150m);
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(servico);
        _repo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _sut.ExecutarAsync(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
