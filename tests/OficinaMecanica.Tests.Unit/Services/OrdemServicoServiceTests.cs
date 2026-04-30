using Moq;
using FluentAssertions;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.Services;

public class OrdemServicoServiceTests
{
    private readonly Mock<IOrdemServicoRepository> _repositoryMock;
    private readonly OrdemServicoService _service;

    public OrdemServicoServiceTests()
    {
        _repositoryMock = new Mock<IOrdemServicoRepository>();
        _service = new OrdemServicoService(_repositoryMock.Object);
    }

    private static OrdemServico CriarOsEmExecucao(Guid clienteId, Guid veiculoId, string obs = "obs")
    {
        var os = new OrdemServico(clienteId, veiculoId, obs);
        os.IniciarDiagnostico("test");
        os.EnviarParaAprovacao("test");
        os.Aprovar("test");
        os.Cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");
        os.Veiculo = new Veiculo(clienteId, "ABC1234", "Toyota", "Corolla", 2020);
        return os;
    }

    // ─── CreateAsync ─────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateOrdemServico()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var osId = Guid.NewGuid();

        var createDto = new CreateOrdemServicoDto
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Observacoes = "Carro com barulho no motor"
        };

        var osCriada = new OrdemServico(clienteId, veiculoId, createDto.Observacoes);
        osCriada.Cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");
        osCriada.Veiculo = new Veiculo(clienteId, "ABC1234", "Toyota", "Corolla", 2020);

        _repositoryMock.Setup(x => x.CriarAsync(It.IsAny<OrdemServico>()))
            .ReturnsAsync(osId);

        _repositoryMock.Setup(x => x.ObterPorIdComItensAsync(osId))
            .ReturnsAsync(osCriada);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.ClienteId.Should().Be(clienteId);
        result.VeiculoId.Should().Be(veiculoId);
        result.Total.Should().Be(0);
        result.Itens.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithEmptyClienteId_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateOrdemServicoDto
        {
            ClienteId = Guid.Empty,
            VeiculoId = Guid.NewGuid()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithEmptyVeiculoId_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateOrdemServicoDto
        {
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.Empty
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
    }

    // ─── GetByIdAsync ────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnOrdemServico()
    {
        // Arrange
        var osId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        var os = CriarOsEmExecucao(clienteId, Guid.NewGuid());

        _repositoryMock.Setup(x => x.ObterPorIdComItensAsync(osId))
            .ReturnsAsync(os);

        // Act
        var result = await _service.GetByIdAsync(osId);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("EmExecucao");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var osId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObterPorIdComItensAsync(osId))
            .ReturnsAsync((OrdemServico?)null);

        // Act
        var result = await _service.GetByIdAsync(osId);

        // Assert
        result.Should().BeNull();
    }

    // ─── AddItemAsync ────────────────────────────────────────

    [Fact]
    public async Task AddItemAsync_WithValidServico_ShouldAddItemAndRecalcularTotal()
    {
        // Arrange
        var osId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var referenciaId = Guid.NewGuid();

        var os = CriarOsEmExecucao(clienteId, Guid.NewGuid());

        var itemDto = new CreateOrdemServicoItemDto
        {
            Tipo = "servico",
            ReferenciaId = referenciaId,
            Descricao = "Troca de Óleo",
            Quantidade = 1,
            PrecoUnitario = 150.00m
        };

        var itemSalvo = new OrdemServicoItem(osId, TipoOSItem.Servico, referenciaId, "Troca de Óleo", 1, 150.00m);

        _repositoryMock.Setup(x => x.ObterPorIdComItensAsync(osId))
            .ReturnsAsync(os);
        _repositoryMock.Setup(x => x.AdicionarItemAsync(It.IsAny<OrdemServicoItem>()))
            .ReturnsAsync(itemSalvo);
        _repositoryMock.Setup(x => x.AtualizarTotalAsync(osId, It.IsAny<decimal>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddItemAsync(osId, itemDto);

        // Assert
        result.Should().NotBeNull();
        result.Descricao.Should().Be("Troca de Óleo");
        result.PrecoUnitario.Should().Be(150.00m);
        result.Subtotal.Should().Be(150.00m);
        result.Tipo.Should().Be("servico");

        _repositoryMock.Verify(x => x.AtualizarTotalAsync(osId, 150.00m), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_WithMultipleItems_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var osId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        var item1 = new OrdemServicoItem(osId, TipoOSItem.Servico, Guid.NewGuid(), "Troca de Óleo", 1, 150.00m);
        var item2 = new OrdemServicoItem(osId, TipoOSItem.Peca, Guid.NewGuid(), "Filtro de Óleo", 2, 35.00m);

        var os = CriarOsEmExecucao(clienteId, Guid.NewGuid());
        os.Itens.Add(item1);

        var novoItemDto = new CreateOrdemServicoItemDto
        {
            Tipo = "peca",
            ReferenciaId = Guid.NewGuid(),
            Descricao = "Filtro de Óleo",
            Quantidade = 2,
            PrecoUnitario = 35.00m
        };

        _repositoryMock.Setup(x => x.ObterPorIdComItensAsync(osId))
            .ReturnsAsync(os);
        _repositoryMock.Setup(x => x.AdicionarItemAsync(It.IsAny<OrdemServicoItem>()))
            .ReturnsAsync(item2);
        _repositoryMock.Setup(x => x.AtualizarTotalAsync(osId, It.IsAny<decimal>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddItemAsync(osId, novoItemDto);

        // Assert: total = 150 + (2 x 35) = 220
        _repositoryMock.Verify(x => x.AtualizarTotalAsync(osId, 220.00m), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_WithInvalidTipo_ShouldThrowException()
    {
        // Arrange
        var osId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        var os = CriarOsEmExecucao(clienteId, Guid.NewGuid());

        var itemDto = new CreateOrdemServicoItemDto
        {
            Tipo = "tipoInvalido",
            ReferenciaId = Guid.NewGuid(),
            Descricao = "Item Teste",
            Quantidade = 1,
            PrecoUnitario = 100.00m
        };

        _repositoryMock.Setup(x => x.ObterPorIdComItensAsync(osId))
            .ReturnsAsync(os);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddItemAsync(osId, itemDto));
    }

    [Fact]
    public async Task AddItemAsync_WithNonExistingOS_ShouldThrowException()
    {
        // Arrange
        var osId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObterPorIdComItensAsync(osId))
            .ReturnsAsync((OrdemServico?)null);

        var itemDto = new CreateOrdemServicoItemDto
        {
            Tipo = "servico",
            ReferenciaId = Guid.NewGuid(),
            Descricao = "Teste",
            Quantidade = 1,
            PrecoUnitario = 100.00m
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AddItemAsync(osId, itemDto));
    }

    // ─── RemoveItemAsync ─────────────────────────────────────

    [Fact]
    public async Task RemoveItemAsync_WithValidItem_ShouldRemoveAndRecalcularTotal()
    {
        // Arrange
        var osId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var item = new OrdemServicoItem(osId, TipoOSItem.Servico, Guid.NewGuid(), "Troca de Óleo", 1, 150.00m);
        typeof(OrdemServicoItem).GetProperty("Id")!.SetValue(item, itemId);

        var os = CriarOsEmExecucao(clienteId, Guid.NewGuid());
        os.Itens.Add(item);

        _repositoryMock.Setup(x => x.ObterPorIdComItensAsync(osId))
            .ReturnsAsync(os);
        _repositoryMock.Setup(x => x.RemoverItemAsync(osId, itemId))
            .Returns(Task.CompletedTask);
        _repositoryMock.Setup(x => x.AtualizarTotalAsync(osId, It.IsAny<decimal>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoveItemAsync(osId, itemId);

        // Assert
        _repositoryMock.Verify(x => x.RemoverItemAsync(osId, itemId), Times.Once);
        _repositoryMock.Verify(x => x.AtualizarTotalAsync(osId, 0), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_WithNonExistingItem_ShouldThrowException()
    {
        // Arrange
        var osId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var os = CriarOsEmExecucao(clienteId, Guid.NewGuid());

        _repositoryMock.Setup(x => x.ObterPorIdComItensAsync(osId))
            .ReturnsAsync(os);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.RemoveItemAsync(osId, itemId));
    }

    // ─── GetTempoMedioExecucaoAsync ──────────────────────────

    [Fact]
    public async Task GetTempoMedioExecucaoAsync_ShouldReturnAverageHours()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetTempoMedioExecucaoHorasAsync())
            .ReturnsAsync(4.5);

        // Act
        var result = await _service.GetTempoMedioExecucaoAsync();

        // Assert
        result.Should().Be(4.5);
    }

    // ─── GetAllAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrdensServico()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        var os1 = new OrdemServico(clienteId, Guid.NewGuid(), "obs1");
        os1.Cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");
        os1.Veiculo = new Veiculo(clienteId, "ABC1234", "Toyota", "Corolla", 2020);

        var os2 = CriarOsEmExecucao(clienteId, Guid.NewGuid(), "obs2");
        os2.Finalizar("test");
        os2.Entregar("test");
        os2.Cliente = new Cliente("Maria Santos", "98765432100", "(11) 98888-8888", "maria@email.com");
        os2.Veiculo = new Veiculo(clienteId, "XYZ9876", "Honda", "Civic", 2022);

        var lista = new List<OrdemServico> { os1, os2 };

        _repositoryMock.Setup(x => x.ListarTodosAsync())
            .ReturnsAsync(lista);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}
