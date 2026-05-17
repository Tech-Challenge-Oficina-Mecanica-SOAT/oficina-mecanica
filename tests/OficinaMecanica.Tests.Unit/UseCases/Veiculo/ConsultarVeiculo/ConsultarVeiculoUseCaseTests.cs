using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Veiculo.ConsultarVeiculo;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Veiculo.ConsultarVeiculo;

public class ConsultarVeiculoUseCaseTests
{
    private readonly Mock<IVeiculoRepository> _repo = new();
    private readonly ConsultarVeiculoUseCase _sut;

    public ConsultarVeiculoUseCaseTests() => _sut = new ConsultarVeiculoUseCase(_repo.Object, new VeiculoMapper());

    [Fact]
    public async Task ExecutarAsync_NaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Veiculo?)null);
        var result = await _sut.ExecutarAsync(Guid.NewGuid());
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrado_RetornaSuccess()
    {
        var v = new Domain.Entities.Veiculo(Guid.NewGuid(), "ABC1234", "Fiat", "Uno", 2020);
        _repo.Setup(r => r.GetByIdAsync(v.Id)).ReturnsAsync(v);

        var result = await _sut.ExecutarAsync(v.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Placa.Should().Be("ABC1234");
    }
}
