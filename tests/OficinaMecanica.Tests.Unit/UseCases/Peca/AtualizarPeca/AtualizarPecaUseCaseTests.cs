using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Peca.AtualizarPeca;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Peca.AtualizarPeca;

public class AtualizarPecaUseCaseTests
{
    private readonly Mock<IPecaInsumoRepository> _repo = new();
    private readonly AtualizarPecaUseCase _sut;

    public AtualizarPecaUseCaseTests() => _sut = new AtualizarPecaUseCase(_repo.Object, new PecaMapper());

    [Fact]
    public async Task ExecutarAsync_NaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PecaInsumo?)null);
        var result = await _sut.ExecutarAsync(new AtualizarPecaUseCaseRequest(Guid.NewGuid(), "n", "d", 10m, 1));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_Atualiza()
    {
        var peca = new PecaInsumo("F", "FIL", "d", 10m, 5);
        _repo.Setup(r => r.GetByIdAsync(peca.Id)).ReturnsAsync(peca);
        _repo.Setup(r => r.UpdateAsync(peca)).ReturnsAsync(peca);

        var result = await _sut.ExecutarAsync(new AtualizarPecaUseCaseRequest(peca.Id, "Novo", "n", 20m, 8));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("Novo");
    }
}
