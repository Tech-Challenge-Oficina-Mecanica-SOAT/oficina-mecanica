using FluentAssertions;
using Moq;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.Services;

public class ServicoServiceTests
{
    private readonly Mock<IServicoRepository> _servicoRepositoryMock;
    private readonly ServicoService _servicoService;

    public ServicoServiceTests()
    {
        _servicoRepositoryMock = new Mock<IServicoRepository>();
        _servicoService = new ServicoService(_servicoRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateServico()
    {
        // Arrange
        var createDto = new CriarServicoRequest
        {
            Nome = "Troca de Óleo",
            Descricao = "Troca de óleo do motor",
            Valor = 150.00m
        };

        _servicoRepositoryMock.Setup(x => x.ExistsByNomeAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _servicoRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Servico>()))
            .ReturnsAsync((Servico s) => s);

        // Act
        var result = await _servicoService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Troca de Óleo");
        result.Valor.Should().Be(150.00m);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidValor_ShouldThrowException()
    {
        // Arrange
        var createDto = new CriarServicoRequest
        {
            Nome = "Serviço Teste",
            Descricao = "Descrição",
            Valor = -10.00m
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _servicoService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithEmptyNome_ShouldThrowException()
    {
        // Arrange
        var createDto = new CriarServicoRequest
        {
            Nome = "",
            Descricao = "Descrição",
            Valor = 100.00m
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _servicoService.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateNome_ShouldThrowException()
    {
        // Arrange
        var createDto = new CriarServicoRequest
        {
            Nome = "Troca de Óleo",
            Descricao = "Troca de óleo do motor",
            Valor = 150.00m
        };

        _servicoRepositoryMock.Setup(x => x.ExistsByNomeAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _servicoService.CreateAsync(createDto));
    }

    [Fact]
    public async Task GetByNomeAsync_ShouldReturnMatchingServicos()
    {
        // Arrange
        var servicos = new List<Servico>
        {
            new Servico("Troca de Óleo", "Troca de óleo", 150.00m),
            new Servico("Óleo do Motor", "Troca de óleo do motor", 200.00m)
        };

        _servicoRepositoryMock.Setup(x => x.GetByNomeAsync("oleo"))
            .ReturnsAsync(servicos);

        // Act
        var result = await _servicoService.GetByNomeAsync("oleo");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Nome.Contains("Óleo"));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnServico()
    {
        // Arrange
        var servicoId = Guid.NewGuid();
        var servico = new Servico("Troca de Óleo", "Troca de óleo", 150.00m);

        _servicoRepositoryMock.Setup(x => x.GetByIdAsync(servicoId))
            .ReturnsAsync(servico);

        // Act
        var result = await _servicoService.GetByIdAsync(servicoId);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Troca de Óleo");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllServicos()
    {
        // Arrange
        var servicos = new List<Servico>
        {
            new Servico("Troca de Óleo", "Troca de óleo", 150.00m),
            new Servico("Alinhamento", "Alinhamento de direção", 120.00m)
        };

        _servicoRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(servicos);

        // Act
        var result = await _servicoService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateServico()
    {
        // Arrange
        var servicoId = Guid.NewGuid();
        var servico = new Servico("Troca de Óleo", "Troca de óleo", 150.00m);
        var updateDto = new AtualizarServicoRequest
        {
            Nome = "Troca de Óleo Sintético",
            Descricao = "Troca de óleo sintético",
            Valor = 250.00m
        };

        _servicoRepositoryMock.Setup(x => x.GetByIdAsync(servicoId))
            .ReturnsAsync(servico);
        _servicoRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Servico>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _servicoService.UpdateAsync(servicoId, updateDto);

        // Assert
        result.Nome.Should().Be("Troca de Óleo Sintético");
        result.Valor.Should().Be(250.00m);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveServico()
    {
        // Arrange
        var servicoId = Guid.NewGuid();
        var servico = new Servico("Troca de Óleo", "Troca de óleo", 150.00m);

        _servicoRepositoryMock.Setup(x => x.GetByIdAsync(servicoId))
            .ReturnsAsync(servico);
        _servicoRepositoryMock.Setup(x => x.DeleteAsync(servicoId))
            .Returns(Task.CompletedTask);

        // Act
        await _servicoService.DeleteAsync(servicoId);

        // Assert
        _servicoRepositoryMock.Verify(x => x.DeleteAsync(servicoId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        _servicoRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Servico?)null);

        var result = await _servicoService.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAtivosAsync_ShouldReturnOnlyActiveServicos()
    {
        var servicos = new List<Servico>
        {
            new Servico("Troca de Óleo", "Descrição", 150.00m),
            new Servico("Alinhamento", "Descrição", 100.00m)
        };

        _servicoRepositoryMock.Setup(x => x.GetAtivosAsync())
            .ReturnsAsync(servicos);

        var result = await _servicoService.GetAtivosAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ShouldThrowKeyNotFoundException()
    {
        _servicoRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Servico?)null);

        var act = () => _servicoService.UpdateAsync(Guid.NewGuid(), new AtualizarServicoRequest
        {
            Nome = "Novo", Descricao = "Desc", Valor = 100m
        });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AtivarAsync_ShouldActivateServico()
    {
        var servicoId = Guid.NewGuid();
        var servico = new Servico("Troca de Óleo", "Descrição", 150.00m);
        servico.Desativar();

        _servicoRepositoryMock.Setup(x => x.GetByIdAsync(servicoId))
            .ReturnsAsync(servico);
        _servicoRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Servico>()))
            .Returns(Task.CompletedTask);

        await _servicoService.AtivarAsync(servicoId);

        servico.Ativo.Should().BeTrue();
        _servicoRepositoryMock.Verify(x => x.UpdateAsync(servico), Times.Once);
    }

    [Fact]
    public async Task AtivarAsync_WithNonExistingId_ShouldThrowKeyNotFoundException()
    {
        _servicoRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Servico?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _servicoService.AtivarAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DesativarAsync_ShouldDeactivateServico()
    {
        var servicoId = Guid.NewGuid();
        var servico = new Servico("Troca de Óleo", "Descrição", 150.00m);

        _servicoRepositoryMock.Setup(x => x.GetByIdAsync(servicoId))
            .ReturnsAsync(servico);
        _servicoRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Servico>()))
            .Returns(Task.CompletedTask);

        await _servicoService.DesativarAsync(servicoId);

        servico.Ativo.Should().BeFalse();
        _servicoRepositoryMock.Verify(x => x.UpdateAsync(servico), Times.Once);
    }

    [Fact]
    public async Task DesativarAsync_WithNonExistingId_ShouldThrowKeyNotFoundException()
    {
        _servicoRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Servico?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _servicoService.DesativarAsync(Guid.NewGuid()));
    }
}
