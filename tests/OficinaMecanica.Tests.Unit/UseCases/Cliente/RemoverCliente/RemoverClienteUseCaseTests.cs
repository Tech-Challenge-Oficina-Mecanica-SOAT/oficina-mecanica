using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.Cliente.RemoverCliente;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Cliente.RemoverCliente;

public class RemoverClienteUseCaseTests
{
    private readonly Mock<IClienteRepository> _repo = new();
    private readonly RemoverClienteUseCase _sut;

    public RemoverClienteUseCaseTests()
    {
        _sut = new RemoverClienteUseCase(_repo.Object);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoClienteNaoExiste_RetornaNotFound()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Domain.Entities.Cliente?)null);

        var result = await _sut.ExecutarAsync(id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoClienteExiste_DeletaERetornaSucesso()
    {
        var id = Guid.NewGuid();
        var cliente = new Domain.Entities.Cliente(
            "João Silva",
            new Domain.ValueObjects.Documento("123.456.789-09"),
            "11999999999",
            new Domain.ValueObjects.Email("joao@email.com"));
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(cliente);
        _repo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _sut.ExecutarAsync(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
