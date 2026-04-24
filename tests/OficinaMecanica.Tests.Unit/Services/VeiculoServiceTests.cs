using Moq;
using FluentAssertions;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.Services;

public class VeiculoServiceTests
{
    private readonly Mock<IVeiculoRepository> _veiculoRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly VeiculoService _veiculoService;

    public VeiculoServiceTests()
    {
        _veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _veiculoService = new VeiculoService(_veiculoRepositoryMock.Object, _clienteRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");
        var createDto = new CreateVeiculoDto
        {
            ClienteId = clienteId,
            Placa = "ABC1234",
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2020
        };

        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(x => x.ExistsByPlacaAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _veiculoRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Veiculo>()))
            .ReturnsAsync((Veiculo v) => v);

        // Act
        var result = await _veiculoService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Placa.Should().Be("ABC1234");
        result.Marca.Should().Be("Fiat");
        result.Modelo.Should().Be("Uno");
        result.Ano.Should().Be(2020);
    }

    [Fact]
    public async Task CreateAsync_WithMercosulPlaca_ShouldCreateVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");
        var createDto = new CreateVeiculoDto
        {
            ClienteId = clienteId,
            Placa = "ABC1D23",
            Marca = "Volkswagen",
            Modelo = "Gol",
            Ano = 2021
        };

        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(x => x.ExistsByPlacaAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _veiculoRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Veiculo>()))
            .ReturnsAsync((Veiculo v) => v);

        // Act
        var result = await _veiculoService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Placa.Should().Be("ABC1D23");
    }

    [Fact]
    public async Task CreateAsync_WithInvalidPlaca_ShouldThrowArgumentException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");
        var createDto = new CreateVeiculoDto
        {
            ClienteId = clienteId,
            Placa = "1234567",  // Placa inválida
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2020
        };

        // Garantir que o cliente existe
        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _veiculoService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicatePlaca_ShouldThrowException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");
        var createDto = new CreateVeiculoDto
        {
            ClienteId = clienteId,
            Placa = "ABC1234",
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2020
        };

        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(x => x.ExistsByPlacaAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _veiculoService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentCliente_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var createDto = new CreateVeiculoDto
        {
            ClienteId = clienteId,
            Placa = "ABC1234",
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2020
        };

        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _veiculoService.CreateAsync(createDto));
    }

    [Fact]
    public async Task GetByPlacaAsync_WithExistingPlaca_ShouldReturnVeiculo()
    {
        // Arrange
        var veiculo = new Veiculo(Guid.NewGuid(), "ABC1234", "Fiat", "Uno", 2020);
        _veiculoRepositoryMock.Setup(x => x.GetByPlacaAsync("ABC1234"))
            .ReturnsAsync(veiculo);

        // Act
        var result = await _veiculoService.GetByPlacaAsync("ABC1234");

        // Assert
        result.Should().NotBeNull();
        result.Placa.Should().Be("ABC1234");
    }

    [Fact]
    public async Task GetByClienteIdAsync_ShouldReturnVeiculosDoCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculos = new List<Veiculo>
        {
            new Veiculo(clienteId, "ABC1234", "Fiat", "Uno", 2020),
            new Veiculo(clienteId, "XYZ9090", "Fiat", "Palio", 2021)
        };

        _veiculoRepositoryMock.Setup(x => x.GetByClienteIdAsync(clienteId))
            .ReturnsAsync(veiculos);

        // Act
        var result = await _veiculoService.GetByClienteIdAsync(clienteId);

        // Assert
        result.Should().HaveCount(2);
    }
}
