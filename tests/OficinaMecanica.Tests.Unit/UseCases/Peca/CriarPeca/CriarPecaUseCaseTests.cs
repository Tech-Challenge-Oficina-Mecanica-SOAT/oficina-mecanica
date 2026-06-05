using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Peca.CriarPeca;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Peca.CriarPeca;

public class CriarPecaUseCaseTests
{
    private readonly Mock<IPecaInsumoRepository> _repo = new();
    private readonly CriarPecaUseCase _sut;

    public CriarPecaUseCaseTests() => _sut = new CriarPecaUseCase(_repo.Object, new PecaMapper());

    private static CriarPecaRequest ValidRequest() => new()
    {
        Nome = "Filtro",
        Codigo = "FIL-001",
        Descricao = "Filtro de oleo",
        PrecoUnitario = 30m,
        Estoque = 10
    };

    [Fact]
    public async Task ExecutarAsync_CodigoDuplicado_RetornaConflict()
    {
        _repo.Setup(r => r.ExistsByCodigoAsync(It.IsAny<string>())).ReturnsAsync(true);
        var result = await _sut.ExecutarAsync(ValidRequest());
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_CriaPeca()
    {
        _repo.Setup(r => r.ExistsByCodigoAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<PecaInsumo>())).ReturnsAsync((PecaInsumo p) => p);

        var result = await _sut.ExecutarAsync(ValidRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("Filtro");
    }
}
