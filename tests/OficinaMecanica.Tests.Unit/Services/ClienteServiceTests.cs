using FluentAssertions;
using Moq;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.Services;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly ClienteService _clienteService;

    public ClienteServiceTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _clienteService = new ClienteService(_clienteRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidCpf_ShouldCreateCliente()
    {
        // Arrange
        var createDto = new CriarClienteRequest
        {
            Nome = "João Silva",
            Documento = "12345678909",
            Telefone = "(11) 99999-9999",
            Email = "joao@email.com"
        };

        _clienteRepositoryMock.Setup(x => x.ExistsByDocumentoAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _clienteRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Cliente>()))
            .ReturnsAsync((Cliente c) => c);

        // Act
        var result = await _clienteService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("João Silva");
        result.Documento.Should().Be("12345678909");
        result.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithCpfWithMask_ShouldCreateCliente()
    {
        // Arrange
        var createDto = new CriarClienteRequest
        {
            Nome = "Maria Santos",
            Documento = "123.456.789-09",
            Telefone = "(11) 98888-8888",
            Email = "maria@email.com"
        };

        _clienteRepositoryMock.Setup(x => x.ExistsByDocumentoAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _clienteRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Cliente>()))
            .ReturnsAsync((Cliente c) => c);

        // Act
        var result = await _clienteService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Documento.Should().Be("12345678909");
    }

    [Fact]
    public async Task CreateAsync_WithInvalidCpf_ShouldThrowException()
    {
        // Arrange
        var createDto = new CriarClienteRequest
        {
            Nome = "João Silva",
            Documento = "11111111111",
            Telefone = "(11) 99999-9999",
            Email = "joao@email.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _clienteService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithValidCnpj_ShouldCreateCliente()
    {
        // Arrange
        var createDto = new CriarClienteRequest
        {
            Nome = "Empresa Teste LTDA",
            Documento = "12345678000195",
            Telefone = "(11) 3333-4444",
            Email = "contato@empresa.com"
        };

        _clienteRepositoryMock.Setup(x => x.ExistsByDocumentoAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _clienteRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Cliente>()))
            .ReturnsAsync((Cliente c) => c);

        // Act
        var result = await _clienteService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Empresa Teste LTDA");
        result.Documento.Should().Be("12345678000195");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateDocument_ShouldThrowException()
    {
        // Arrange
        var createDto = new CriarClienteRequest
        {
            Nome = "João Silva",
            Documento = "12345678909",
            Telefone = "(11) 99999-9999",
            Email = "joao@email.com"
        };

        _clienteRepositoryMock.Setup(x => x.ExistsByDocumentoAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _clienteService.CreateAsync(createDto));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");

        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        // Act
        var result = await _clienteService.GetByIdAsync(clienteId);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("João Silva");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _clienteService.GetByIdAsync(clienteId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllClientes()
    {
        // Arrange
        var clientes = new List<Cliente>
        {
            new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com"),
            new Cliente("Maria Santos", "98765432100", "(11) 98888-8888", "maria@email.com")
        };

        _clienteRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(clientes);

        // Act
        var result = await _clienteService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");
        var updateDto = new AtualizarClienteRequest
        {
            Nome = "João Santos",
            Telefone = "(11) 98888-8888",
            Email = "joao.santos@email.com"
        };

        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Cliente>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _clienteService.UpdateAsync(clienteId, updateDto);

        // Assert
        result.Nome.Should().Be("João Santos");
        result.Telefone.Should().Be("(11) 98888-8888");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");

        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock.Setup(x => x.DeleteAsync(clienteId))
            .Returns(Task.CompletedTask);

        // Act
        await _clienteService.DeleteAsync(clienteId);

        // Assert
        _clienteRepositoryMock.Verify(x => x.DeleteAsync(clienteId), Times.Once);
    }

    [Fact]
    public async Task DesativarAsync_ShouldDeactivateCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");

        _clienteRepositoryMock.Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Cliente>()))
            .Returns(Task.CompletedTask);

        // Act
        await _clienteService.DesativarAsync(clienteId);

        // Assert
        cliente.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task GetByDocumentoAsync_WhenClienteExists_ReturnsClienteDto()
    {
        // Arrange
        var documento = "39053344705";
        var clienteEntity = new Cliente("João Silva", documento, "11999999999", "joao@email.com");
        var clienteId = Guid.NewGuid();
        var reflexao = typeof(Cliente).GetProperty("Id")!.DeclaringType!.GetProperty("Id");

        _clienteRepositoryMock
            .Setup(r => r.GetByDocumentoAsync(documento))
            .ReturnsAsync(clienteEntity);

        // Act
        var result = await _clienteService.GetByDocumentoAsync(documento);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(clienteEntity.Id, result.Id);
        Assert.Equal(clienteEntity.Nome, result.Nome);
        Assert.Equal(clienteEntity.Documento, result.Documento);
        Assert.Equal(clienteEntity.Telefone, result.Telefone);
        Assert.Equal(clienteEntity.Email, result.Email);
        Assert.Equal(clienteEntity.Ativo, result.Ativo);
        Assert.Equal(clienteEntity.CriadoEm, result.CriadoEm);
        _clienteRepositoryMock.Verify(r => r.GetByDocumentoAsync(documento), Times.Once);
    }

    [Fact]
    public async Task GetByDocumentoAsync_WhenClienteNotExists_ReturnsNull()
    {
        // Arrange
        var documento = "39053344705";
        _clienteRepositoryMock
            .Setup(r => r.GetByDocumentoAsync(documento))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _clienteService.GetByDocumentoAsync(documento);

        // Assert
        Assert.Null(result);
        _clienteRepositoryMock.Verify(r => r.GetByDocumentoAsync(documento), Times.Once);
    }

    [Fact]
    public async Task AtivarAsync_DeveLancarExcecao_QuandoClienteNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _clienteService.AtivarAsync(id));
    }

    [Fact]
    public async Task AtivarAsync_DeveAtivarCliente_QuandoClienteExiste()
    {
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");

        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.Id))
            .ReturnsAsync(cliente);

        _clienteRepositoryMock
            .Setup(r => r.UpdateAsync(cliente))
            .Returns(Task.CompletedTask);

        // Act
        await _clienteService.AtivarAsync(cliente.Id);

        // Assert
        Assert.True(cliente.Ativo);
        _clienteRepositoryMock.Verify(r => r.UpdateAsync(cliente), Times.Once);
    }

    [Fact]
    public async Task AtivarAsync_DevePropagarExcecao_SeUpdateFalhar()
    {
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");

        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.Id))
            .ReturnsAsync(cliente);

        _clienteRepositoryMock
            .Setup(r => r.UpdateAsync(cliente))
            .ThrowsAsync(new InvalidOperationException("Erro ao atualizar"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _clienteService.AtivarAsync(cliente.Id));
    }

    [Fact]
    public async Task DesativarAsync_DeveLancarExcecao_QuandoClienteNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _clienteService.DesativarAsync(id));
    }

    [Fact]
    public async Task DesativarAsync_DeveDesativarCliente_QuandoClienteExiste()
    {
        var cliente = new Cliente("João Silva", "12345678909", "(11) 99999-9999", "joao@email.com");
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.Id))
            .ReturnsAsync(cliente);

        _clienteRepositoryMock
            .Setup(r => r.UpdateAsync(cliente))
            .Returns(Task.CompletedTask);

        // Act
        await _clienteService.DesativarAsync(cliente.Id);

        // Assert
        Assert.False(cliente.Ativo);
        _clienteRepositoryMock.Verify(r => r.UpdateAsync(cliente), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoClienteNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new AtualizarClienteRequest
        {
            Nome = "Novo Nome",
            Telefone = "11999999999",
            Email = "teste@teste.com"
        };

        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        var excecao = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _clienteService.UpdateAsync(id, updateDto)
        );

        Assert.Equal("Cliente não encontrado", excecao.Message);
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarCliente_QuandoClienteExiste()
    {
        var cliente = new Cliente("Antigo Nome","12345678909","11888888888","antigo@teste.com");

        var updateDto = new AtualizarClienteRequest
        {
            Nome = "Novo Nome",
            Telefone = "11999999999",
            Email = "novo@teste.com"
        };

        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.Id))
            .ReturnsAsync(cliente);

        _clienteRepositoryMock
            .Setup(r => r.UpdateAsync(cliente))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _clienteService.UpdateAsync(cliente.Id, updateDto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Novo Nome", resultado.Nome);
        Assert.Equal("11999999999", resultado.Telefone);
        Assert.Equal("novo@teste.com", resultado.Email);
        _clienteRepositoryMock.Verify(r => r.UpdateAsync(cliente), Times.Once);
    }
}
