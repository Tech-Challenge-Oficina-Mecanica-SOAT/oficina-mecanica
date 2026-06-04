using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Veiculo.AtualizarVeiculo;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.UseCases.Veiculo.AtualizarVeiculo;

public class AtualizarVeiculoUseCaseTests
{
    private readonly Mock<IVeiculoRepository> _veiculoRepo = new();
    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly AtualizarVeiculoUseCase _sut;

    public AtualizarVeiculoUseCaseTests() =>
        _sut = new AtualizarVeiculoUseCase(_veiculoRepo.Object, _clienteRepo.Object, new VeiculoMapper());

    [Fact]
    public async Task ExecutarAsync_VeiculoNaoEncontrado_RetornaNotFound()
    {
        _veiculoRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Veiculo?)null);
        var result = await _sut.ExecutarAsync(new AtualizarVeiculoUseCaseRequest(Guid.NewGuid(), null, "ABC1234", "M", "Mo", 2020));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_Atualiza()
    {
        var clienteId = Guid.NewGuid();
        var veiculo = new Domain.Entities.Veiculo(clienteId, "ABC1234", "Fiat", "Uno", 2020);
        _veiculoRepo.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _veiculoRepo.Setup(r => r.ExistsByPlacaForOtherVeiculoAsync(It.IsAny<string>(), veiculo.Id)).ReturnsAsync(false);
        _veiculoRepo.Setup(r => r.UpdateAsync(veiculo)).ReturnsAsync(veiculo);

        var result = await _sut.ExecutarAsync(new AtualizarVeiculoUseCaseRequest(veiculo.Id, null, "XYZ9876", "Fiat", "UnoX", 2021));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Modelo.Should().Be("UnoX");
    }
}
