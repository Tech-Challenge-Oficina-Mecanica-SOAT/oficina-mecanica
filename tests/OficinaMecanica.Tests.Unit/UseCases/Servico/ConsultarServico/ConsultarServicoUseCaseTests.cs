using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Servico.ConsultarServico;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Servico.ConsultarServico;

public class ConsultarServicoUseCaseTests
{
    private readonly Mock<IServicoRepository> _repo = new();
    private readonly ConsultarServicoUseCase _sut;

    public ConsultarServicoUseCaseTests() => _sut = new ConsultarServicoUseCase(_repo.Object, new ServicoMapper());

    [Fact]
    public async Task ExecutarAsync_NaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Servico?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrado_RetornaSuccess()
    {
        var s = new Domain.Entities.Servico("Troca", "d", 50m);
        _repo.Setup(r => r.GetByIdAsync(s.Id)).ReturnsAsync(s);

        var result = await _sut.ExecutarAsync(s.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("Troca");
    }
}
