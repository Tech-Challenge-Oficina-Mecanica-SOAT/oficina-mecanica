using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Peca.ConsultarPeca;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Peca.ConsultarPeca;

public class ConsultarPecaUseCaseTests
{
    private readonly Mock<IPecaInsumoRepository> _repo = new();
    private readonly ConsultarPecaUseCase _sut;

    public ConsultarPecaUseCaseTests() => _sut = new ConsultarPecaUseCase(_repo.Object, new PecaMapper());

    [Fact]
    public async Task ExecutarAsync_NaoEncontrada_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PecaInsumo?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrada_RetornaSuccess()
    {
        var peca = new PecaInsumo("F", "FIL", "d", 10m, 5);
        _repo.Setup(r => r.GetByIdAsync(peca.Id)).ReturnsAsync(peca);

        var result = await _sut.ExecutarAsync(peca.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("F");
    }
}
