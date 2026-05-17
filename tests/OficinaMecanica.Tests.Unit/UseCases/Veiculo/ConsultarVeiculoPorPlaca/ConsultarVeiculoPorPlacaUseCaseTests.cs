using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Veiculo.ConsultarVeiculoPorPlaca;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Veiculo.ConsultarVeiculoPorPlaca;

public class ConsultarVeiculoPorPlacaUseCaseTests
{
    private readonly Mock<IVeiculoRepository> _repo = new();
    private readonly ConsultarVeiculoPorPlacaUseCase _sut;

    public ConsultarVeiculoPorPlacaUseCaseTests() => _sut = new ConsultarVeiculoPorPlacaUseCase(_repo.Object, new VeiculoMapper());

    [Fact]
    public async Task ExecutarAsync_NaoEncontrado_RetornaNotFound()
    {
        _repo.Setup(r => r.GetByPlacaAsync(It.IsAny<string>())).ReturnsAsync((Domain.Entities.Veiculo?)null);
        var result = await _sut.ExecutarAsync("ABC1234");
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Encontrado_RetornaSuccess()
    {
        var v = new Domain.Entities.Veiculo(Guid.NewGuid(), "ABC1234", "Fiat", "Uno", 2020);
        _repo.Setup(r => r.GetByPlacaAsync("ABC1234")).ReturnsAsync(v);

        var result = await _sut.ExecutarAsync("ABC1234");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Placa.Should().Be("ABC1234");
    }
}
