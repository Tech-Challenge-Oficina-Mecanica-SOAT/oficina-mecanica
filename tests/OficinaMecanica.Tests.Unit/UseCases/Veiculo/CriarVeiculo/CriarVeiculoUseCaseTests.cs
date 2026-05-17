using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Veiculo.CriarVeiculo;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Tests.Unit.UseCases.Veiculo.CriarVeiculo;

public class CriarVeiculoUseCaseTests
{
    private readonly Mock<IVeiculoRepository> _veiculoRepo = new();
    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly CriarVeiculoUseCase _sut;

    public CriarVeiculoUseCaseTests() =>
        _sut = new CriarVeiculoUseCase(_veiculoRepo.Object, _clienteRepo.Object, new VeiculoMapper());

    private static CriarVeiculoRequest Req(Guid clienteId) => new()
    {
        ClienteId = clienteId,
        Placa = "ABC1234",
        Marca = "Fiat",
        Modelo = "Uno",
        Ano = 2020
    };

    [Fact]
    public async Task ExecutarAsync_ClienteNaoEncontrado_RetornaNotFound()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Cliente?)null);
        var result = await _sut.ExecutarAsync(Req(Guid.NewGuid()));
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task ExecutarAsync_PlacaDuplicada_RetornaConflict()
    {
        var cli = new Domain.Entities.Cliente("J", new Documento("12345678909"), "1", new Email("a@b.com"));
        _clienteRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cli);
        _veiculoRepo.Setup(r => r.ExistsByPlacaAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await _sut.ExecutarAsync(Req(cli.Id));

        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task ExecutarAsync_Valido_CriaVeiculo()
    {
        var cli = new Domain.Entities.Cliente("J", new Documento("12345678909"), "1", new Email("a@b.com"));
        _clienteRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cli);
        _veiculoRepo.Setup(r => r.ExistsByPlacaAsync(It.IsAny<string>())).ReturnsAsync(false);
        _veiculoRepo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Veiculo>()))
            .ReturnsAsync((Domain.Entities.Veiculo v) => v);

        var result = await _sut.ExecutarAsync(Req(cli.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Placa.Should().Be("ABC1234");
    }
}
