using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Cliente.AtualizarCliente;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.UseCases.Cliente.AtualizarCliente;

public class AtualizarClienteUseCaseTests
{
    private readonly Mock<IClienteRepository> _repo = new();
    private readonly AtualizarClienteUseCase _sut;

    public AtualizarClienteUseCaseTests()
    {
        _sut = new AtualizarClienteUseCase(_repo.Object, new ClienteMapper());
    }

    [Fact]
    public async Task ExecutarAsync_ClienteNaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Cliente?)null);
        var result = await _sut.ExecutarAsync(new AtualizarClienteUseCaseRequest(Guid.NewGuid(), "n", "1", "a@b.com"));
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_DadosValidos_AtualizaCliente()
    {
        var cliente = new Domain.Entities.Cliente("Joao", new Documento("12345678909"), new Telefone("11911223344"), new Email("a@b.com"));
        _repo.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);

        var result = await _sut.ExecutarAsync(new AtualizarClienteUseCaseRequest(cliente.Id, "Joao Silva", "11988776655", "b@c.com"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("Joao Silva");
        _repo.Verify(r => r.UpdateAsync(cliente), Times.Once);
    }
}
