using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Servico.CriarServico;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Servico.CriarServico;

public class CriarServicoUseCaseTests
{
    private readonly Mock<IServicoRepository> _repo = new();
    private readonly CriarServicoUseCase _sut;

    public CriarServicoUseCaseTests() => _sut = new CriarServicoUseCase(_repo.Object, new ServicoMapper());

    [Fact]
    public async Task ExecutarAsync_NomeDuplicado_RetornaConflict()
    {
        _repo.Setup(r => r.ExistsByNomeAsync(It.IsAny<string>())).ReturnsAsync(true);
        var result = await _sut.ExecutarAsync(new CriarServicoRequest { Nome = "Troca de oleo", Valor = 50 });
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task ExecutarAsync_ValorInvalido_RetornaValidation()
    {
        _repo.Setup(r => r.ExistsByNomeAsync(It.IsAny<string>())).ReturnsAsync(false);
        var result = await _sut.ExecutarAsync(new CriarServicoRequest { Nome = "Troca", Valor = 0 });
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task ExecutarAsync_DadosValidos_CriaServico()
    {
        _repo.Setup(r => r.ExistsByNomeAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Servico>()))
            .ReturnsAsync((Domain.Entities.Servico s) => s);

        var result = await _sut.ExecutarAsync(new CriarServicoRequest { Nome = "Troca", Descricao = "d", Valor = 50 });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("Troca");
    }
}
