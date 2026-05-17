using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Peca.AtualizarEstoque;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Peca.AtualizarEstoque;

public class AtualizarEstoqueUseCaseTests
{
    private readonly Mock<IPecaInsumoRepository> _repo = new();
    private readonly AtualizarEstoqueUseCase _sut;

    public AtualizarEstoqueUseCaseTests() => _sut = new AtualizarEstoqueUseCase(_repo.Object, new PecaMapper());

    [Fact]
    public async Task ExecutarAsync_NaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PecaInsumo?)null);
        var result = await _sut.ExecutarAsync(new AtualizarEstoqueUseCaseRequest(Guid.NewGuid(), 5, "incrementar"));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_TipoOperacaoInvalido_RetornaValidation()
    {
        var peca = new PecaInsumo("F", "FIL", "d", 1m, 1);
        _repo.Setup(r => r.GetByIdAsync(peca.Id)).ReturnsAsync(peca);

        var result = await _sut.ExecutarAsync(new AtualizarEstoqueUseCaseRequest(peca.Id, 5, "foo"));

        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_Incrementar_AtualizaEstoque()
    {
        var peca = new PecaInsumo("F", "FIL", "d", 1m, 1);
        _repo.Setup(r => r.GetByIdAsync(peca.Id)).ReturnsAsync(peca);
        _repo.Setup(r => r.IncrementarEstoqueAsync(peca.Id, 5)).ReturnsAsync(peca);

        var result = await _sut.ExecutarAsync(new AtualizarEstoqueUseCaseRequest(peca.Id, 5, "incrementar"));

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.IncrementarEstoqueAsync(peca.Id, 5), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_Decrementar_AtualizaEstoque()
    {
        var peca = new PecaInsumo("F", "FIL", "d", 1m, 10);
        _repo.Setup(r => r.GetByIdAsync(peca.Id)).ReturnsAsync(peca);
        _repo.Setup(r => r.DecrementarEstoqueAsync(peca.Id, 3)).ReturnsAsync(peca);

        var result = await _sut.ExecutarAsync(new AtualizarEstoqueUseCaseRequest(peca.Id, -3, "decrementar"));

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.DecrementarEstoqueAsync(peca.Id, 3), Times.Once);
    }
}
