using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Cliente.ConsultarCliente;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.UseCases.Cliente.ConsultarCliente;

public class ConsultarClienteUseCaseTests
{
    private readonly Mock<IClienteRepository> _repo = new();
    private readonly ConsultarClienteUseCase _sut;

    public ConsultarClienteUseCaseTests()
    {
        _sut = new ConsultarClienteUseCase(_repo.Object, new ClienteMapper());
    }

    [Fact]
    public async Task ExecutarAsync_NaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Cliente?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrado_RetornaSuccess()
    {
        var cliente = new Domain.Entities.Cliente("Joao", new Documento("12345678909"), new Telefone("11911223344"), new Email("a@b.com"));
        _repo.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);

        var result = await _sut.ExecutarAsync(cliente.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("Joao");
    }
}
