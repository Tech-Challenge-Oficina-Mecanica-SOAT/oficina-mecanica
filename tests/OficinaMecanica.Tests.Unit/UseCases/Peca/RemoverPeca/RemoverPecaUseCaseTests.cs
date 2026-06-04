using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.Peca.RemoverPeca;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Peca.RemoverPeca;

public class RemoverPecaUseCaseTests
{
    private readonly Mock<IPecaInsumoRepository> _repo = new();
    private readonly RemoverPecaUseCase _sut;

    public RemoverPecaUseCaseTests() => _sut = new RemoverPecaUseCase(_repo.Object);

    [Fact]
    public async Task ExecutarAsync_NaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PecaInsumo?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrada_Remove()
    {
        var peca = new PecaInsumo("F", "FIL", "d", 1m, 1);
        _repo.Setup(r => r.GetByIdAsync(peca.Id)).ReturnsAsync(peca);

        var result = await _sut.ExecutarAsync(peca.Id);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(peca.Id), Times.Once);
    }
}
