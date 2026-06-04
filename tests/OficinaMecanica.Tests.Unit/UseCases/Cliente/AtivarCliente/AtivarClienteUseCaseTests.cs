using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.Cliente.AtivarCliente;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.UseCases.Cliente.AtivarCliente;

public class AtivarClienteUseCaseTests
{
    private readonly Mock<IClienteRepository> _repo = new();
    private readonly AtivarClienteUseCase _sut;

    public AtivarClienteUseCaseTests() => _sut = new AtivarClienteUseCase(_repo.Object);

    [Fact]
    public async Task ExecutarAsync_NaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Cliente?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrado_AtivaCliente()
    {
        var c = new Domain.Entities.Cliente("Joao", new Documento("12345678909"), new Telefone("11911223344"), new Email("a@b.com"));
        c.Desativar();
        _repo.Setup(r => r.GetByIdAsync(c.Id)).ReturnsAsync(c);

        var result = await _sut.ExecutarAsync(c.Id);

        result.IsSuccess.Should().BeTrue();
        c.Ativo.Should().BeTrue();
    }
}
