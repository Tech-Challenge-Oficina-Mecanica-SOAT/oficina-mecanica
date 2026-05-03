using FluentAssertions;
using Moq;
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

    [Fact]
    public async Task GetByIdAsync_DeveRetornarNull_QuandoVeiculoNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        _veiculoRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Veiculo)null);

        // Act
        var resultado = await _veiculoService.GetByIdAsync(id);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarDto_QuandoVeiculoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        var veiculo = new Veiculo(id, "ABC1234", "Fiat", "Uno", 2020);
        _veiculoRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(veiculo);

        // Act
        var resultado = await _veiculoService.GetByIdAsync(id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado.ClienteId);
        Assert.Equal("ABC1234", resultado.Placa);
        Assert.Equal("Fiat", resultado.Marca);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarListaVazia_QuandoNaoExistemVeiculos()
    {
        // Arrange
        _veiculoRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Veiculo>());

        // Act
        var resultado = await _veiculoService.GetAllAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarListaDeDtos_QuandoExistemVeiculos()
    {
        // Arrange
        var veiculos = new List<Veiculo>
        {
            new Veiculo ( Guid.NewGuid(), "ABC1234", "Fiesta", "Black" , 2020),
            new Veiculo ( Guid.NewGuid(), "XYZ5678", "Civic", "Black" , 2020 )
        };

        _veiculoRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(veiculos);

        // Act
        var resultado = await _veiculoService.GetAllAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count());

        var dto1 = resultado.First();
        Assert.Equal("ABC1234", dto1.Placa);
        Assert.Equal("Fiesta", dto1.Marca);
        Assert.Equal(2020, dto1.Ano);

        var dto2 = resultado.Last();
        Assert.Equal("XYZ5678", dto2.Placa);
        Assert.Equal("Civic", dto2.Marca);
    }

    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoVeiculoNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateVeiculoDto { Placa = "ABC1234" };

        _veiculoRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Veiculo)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _veiculoService.UpdateAsync(id, updateDto)
        );

        Assert.Contains("Veículo com ID", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoClienteNaoExiste()
    {
        var veiculo = new Veiculo(Guid.NewGuid(), "ABC1234", "Fiat", "Uno", 2020);
        var updateDto = new UpdateVeiculoDto
        {
            ClienteId = Guid.NewGuid(),
            Placa = "BBB2222"
        };

        _veiculoRepositoryMock
            .Setup(r => r.GetByIdAsync(veiculo.Id))
            .ReturnsAsync(veiculo);

        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(updateDto.ClienteId.Value))
            .ReturnsAsync((Cliente)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _veiculoService.UpdateAsync(veiculo.Id, updateDto)
        );

        Assert.Contains("Cliente com ID", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoPlacaJaExisteEmOutroVeiculo()
    {
        
        var veiculo = new Veiculo(Guid.NewGuid(), "ABC1234", "Fiat", "Uno", 2020);
        var updateDto = new UpdateVeiculoDto { Placa = "CCC3333" };

        _veiculoRepositoryMock
            .Setup(r => r.GetByIdAsync(veiculo.Id))
            .ReturnsAsync(veiculo);

        _veiculoRepositoryMock
            .Setup(r => r.ExistsByPlacaForOtherVeiculoAsync(updateDto.Placa, veiculo.Id))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _veiculoService.UpdateAsync(veiculo.Id, updateDto)
        );

        Assert.Contains("Veículo com placa", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarVeiculo_QuandoDadosValidos()
    {
        var veiculo = new Veiculo(Guid.NewGuid(), "ABC1234", "Fiat", "Uno", 2020);
        var updateDto = new UpdateVeiculoDto
        {
            Placa = "DDD4444",
            Marca = "Ford",
            Modelo = "Fiesta",
            Ano = 2020
        };

        _veiculoRepositoryMock
            .Setup(r => r.GetByIdAsync(veiculo.Id))
            .ReturnsAsync(veiculo);

        _veiculoRepositoryMock
            .Setup(r => r.ExistsByPlacaForOtherVeiculoAsync(updateDto.Placa, veiculo.Id))
            .ReturnsAsync(false);

        _veiculoRepositoryMock
            .Setup(r => r.UpdateAsync(veiculo))
            .ReturnsAsync(veiculo);

        // Act
        var resultado = await _veiculoService.UpdateAsync(veiculo.Id, updateDto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("DDD4444", resultado.Placa);
        Assert.Equal("Fiesta", resultado.Modelo);
        Assert.Equal(2020, resultado.Ano);
        _veiculoRepositoryMock.Verify(r => r.UpdateAsync(veiculo), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DeveChamarRepositorioDelete()
    {
        // Arrange
        var id = Guid.NewGuid();

        _veiculoRepositoryMock
            .Setup(r => r.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        await _veiculoService.DeleteAsync(id);

        // Assert
        _veiculoRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
