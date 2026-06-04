using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.UseCases.Peca.ObterEstoque;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Peca.ObterEstoque;

public class ObterEstoqueUseCaseTests
{
    private readonly Mock<IPecaInsumoRepository> _repo = new();
    private readonly ObterEstoqueUseCase _sut;

    public ObterEstoqueUseCaseTests() => _sut = new ObterEstoqueUseCase(_repo.Object);

    [Fact]
    public async Task ExecutarAsync_NaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PecaInsumo?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrada_RetornaEstoque()
    {
        var peca = new PecaInsumo("F", "FIL", "d", 1m, 7);
        _repo.Setup(r => r.GetByIdAsync(peca.Id)).ReturnsAsync(peca);
        _repo.Setup(r => r.GetEstoqueAsync(peca.Id)).ReturnsAsync(7);

        var result = await _sut.ExecutarAsync(peca.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7);
    }
}
