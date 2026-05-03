using FluentAssertions;
using Moq;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.Services;

public class PecaInsumoServiceTests
{
    private readonly Mock<IPecaInsumoRepository> _pecaRepositoryMock;
    private readonly PecaService _pecaService;

    public PecaInsumoServiceTests()
    {
        _pecaRepositoryMock = new Mock<IPecaInsumoRepository>();
        _pecaService = new PecaService(_pecaRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreatePeca()
    {
        // Arrange
        var createDto = new CreatePecaDto
        {
            Nome = "Filtro de Óleo",
            Codigo = "FO-123",
            Descricao = "Filtro de óleo para motor",
            PrecoUnitario = 45.90m,
            Estoque = 10
        };

        _pecaRepositoryMock.Setup(x => x.ExistsByCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _pecaRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PecaInsumo>()))
            .ReturnsAsync((PecaInsumo p) => p);

        // Act
        var result = await _pecaService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Filtro de Óleo");
        result.Codigo.Should().Be("FO-123");
        result.Descricao.Should().Be("Filtro de óleo para motor");
        result.PrecoUnitario.Should().Be(45.90m);
        result.Estoque.Should().Be(10);
    }

    [Fact]
    public async Task CreateAsync_WithNegativeEstoque_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreatePecaDto
        {
            Nome = "Peça Teste",
            Codigo = "PT-001",
            Descricao = "Descrição da peça",
            PrecoUnitario = 100.00m,
            Estoque = -5
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _pecaService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateCodigo_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreatePecaDto
        {
            Nome = "Filtro de Óleo",
            Codigo = "FO-123",
            Descricao = "Filtro de óleo para motor",
            PrecoUnitario = 45.90m,
            Estoque = 10
        };

        _pecaRepositoryMock.Setup(x => x.ExistsByCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _pecaService.CreateAsync(createDto));
    }

    [Fact]
    public async Task GetByNomeAsync_ShouldReturnMatchingPecas()
    {
        // Arrange
        var pecas = new List<PecaInsumo>
        {
            new PecaInsumo("Filtro de Óleo", "FO-123", "Descrição do filtro", 45.90m, 10),
            new PecaInsumo("Filtro de Ar", "FA-456", "Descrição do filtro de ar", 35.90m, 15)
        };

        _pecaRepositoryMock.Setup(x => x.GetByNomeAsync("filtro"))
            .ReturnsAsync(pecas);

        // Act
        var result = await _pecaService.GetByNomeAsync("filtro");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Nome.Contains("Filtro"));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnPeca()
    {
        // Arrange
        var pecaId = Guid.NewGuid();
        var peca = new PecaInsumo("Filtro de Óleo", "FO-123", "Descrição do filtro", 45.90m, 10);

        _pecaRepositoryMock.Setup(x => x.GetByIdAsync(pecaId))
            .ReturnsAsync(peca);

        // Act
        var result = await _pecaService.GetByIdAsync(pecaId);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Filtro de Óleo");
        result.Descricao.Should().Be("Descrição do filtro");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPecas()
    {
        // Arrange
        var pecas = new List<PecaInsumo>
        {
            new PecaInsumo("Filtro de Óleo", "FO-123", "Descrição do filtro", 45.90m, 10),
            new PecaInsumo("Pastilha de Freio", "PF-789", "Descrição da pastilha", 89.90m, 20)
        };

        _pecaRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(pecas);

        // Act
        var result = await _pecaService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePeca()
    {
        // Arrange
        var pecaId = Guid.NewGuid();
        var peca = new PecaInsumo("Filtro de Óleo", "FO-123", "Descrição original", 45.90m, 10);
        var updateDto = new UpdatePecaDto
        {
            Nome = "Filtro de Óleo Premium",
            Descricao = "Descrição atualizada",
            PrecoUnitario = 55.90m,
            Estoque = 20
        };

        _pecaRepositoryMock.Setup(x => x.GetByIdAsync(pecaId))
            .ReturnsAsync(peca);
        _pecaRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<PecaInsumo>()))
            .ReturnsAsync(peca);

        // Act
        var result = await _pecaService.UpdateAsync(pecaId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Filtro de Óleo Premium");
        result.Descricao.Should().Be("Descrição atualizada");
        result.PrecoUnitario.Should().Be(55.90m);
        result.Estoque.Should().Be(20);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemovePeca()
    {
        // Arrange
        var pecaId = Guid.NewGuid();
        var peca = new PecaInsumo("Filtro de Óleo", "FO-123", "Descrição do filtro", 45.90m, 10);

        _pecaRepositoryMock.Setup(x => x.GetByIdAsync(pecaId))
            .ReturnsAsync(peca);
        _pecaRepositoryMock.Setup(x => x.DeleteAsync(pecaId))
            .Returns(Task.CompletedTask);

        // Act
        await _pecaService.DeleteAsync(pecaId);

        // Assert
        _pecaRepositoryMock.Verify(x => x.DeleteAsync(pecaId), Times.Once);
    }

    [Fact]
    public async Task UpdateEstoqueAsync_WithIncrement_ShouldIncreaseStock()
    {
        // Arrange
        var pecaId = Guid.NewGuid();
        var peca = new PecaInsumo("Filtro de Óleo", "FO-123", "Descrição do filtro", 45.90m, 10);
        var updateEstoqueDto = new UpdateEstoqueDto
        {
            Quantidade = 5,
            TipoOperacao = "incrementar"
        };

        _pecaRepositoryMock.Setup(x => x.GetByIdAsync(pecaId))
            .ReturnsAsync(peca);
        _pecaRepositoryMock.Setup(x => x.IncrementarEstoqueAsync(pecaId, 5))
            .ReturnsAsync(peca);

        // Act
        var result = await _pecaService.UpdateEstoqueAsync(pecaId, updateEstoqueDto);

        // Assert
        _pecaRepositoryMock.Verify(x => x.IncrementarEstoqueAsync(pecaId, 5), Times.Once);
    }

    [Fact]
    public async Task UpdateEstoqueAsync_WithDecrement_ShouldDecreaseStock()
    {
        // Arrange
        var pecaId = Guid.NewGuid();
        var peca = new PecaInsumo("Filtro de Óleo", "FO-123", "Descrição do filtro", 45.90m, 10);
        var updateEstoqueDto = new UpdateEstoqueDto
        {
            Quantidade = 3,
            TipoOperacao = "decrementar"
        };

        _pecaRepositoryMock.Setup(x => x.GetByIdAsync(pecaId))
            .ReturnsAsync(peca);
        _pecaRepositoryMock.Setup(x => x.DecrementarEstoqueAsync(pecaId, 3))
            .ReturnsAsync(peca);

        // Act
        var result = await _pecaService.UpdateEstoqueAsync(pecaId, updateEstoqueDto);

        // Assert
        _pecaRepositoryMock.Verify(x => x.DecrementarEstoqueAsync(pecaId, 3), Times.Once);
    }

    [Fact]
    public async Task GetEstoqueAsync_ShouldReturnCurrentStock()
    {
        // Arrange
        var pecaId = Guid.NewGuid();
        _pecaRepositoryMock.Setup(x => x.GetEstoqueAsync(pecaId))
            .ReturnsAsync(15);

        // Act
        var result = await _pecaService.GetEstoqueAsync(pecaId);

        // Assert
        result.Should().Be(15);
    }

    [Fact]
    public async Task GetByEstoqueBaixoAsync_ShouldReturnPecasWithLowStock()
    {
        // Arrange
        var pecas = new List<PecaInsumo>
        {
            new PecaInsumo("Filtro de Óleo", "FO-123", "Descrição do filtro", 45.90m, 3),
            new PecaInsumo("Pastilha de Freio", "PF-789", "Descrição da pastilha", 89.90m, 5)
        };

        _pecaRepositoryMock.Setup(x => x.GetByEstoqueBaixoAsync(10))
            .ReturnsAsync(pecas);

        // Act
        var result = await _pecaService.GetByEstoqueBaixoAsync(10);

        // Assert
        result.Should().HaveCount(2);
    }
}