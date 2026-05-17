using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Tests.Unit.DTOs;


public class VeiculoResponseTests
{
    [Fact]
    public void VeiculoDto_QuandoCriado_DeveInicializarComValoresPadrao()
    {
        // Arrange & Act
        var dto = new VeiculoResponse();

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
        Assert.Equal(Guid.Empty, dto.ClienteId);
        Assert.Empty(dto.ClienteNome);
        Assert.Empty(dto.Placa);
        Assert.Empty(dto.Marca);
        Assert.Empty(dto.Modelo);
        Assert.Equal(0, dto.Ano);
        Assert.Equal(default, dto.CriadoEm);
    }

    [Fact]
    public void VeiculoDto_QuandoPopulado_DeveArmazenarValoresCorretamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var data = DateTime.Now;

        var dto = new VeiculoResponse
        {
            Id = id,
            ClienteId = clienteId,
            ClienteNome = "João Silva",
            Placa = "ABC-1234",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            CriadoEm = data
        };

        // Act & Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(clienteId, dto.ClienteId);
        Assert.Equal("João Silva", dto.ClienteNome);
        Assert.Equal("ABC-1234", dto.Placa);
        Assert.Equal("Toyota", dto.Marca);
        Assert.Equal("Corolla", dto.Modelo);
        Assert.Equal(2023, dto.Ano);
        Assert.Equal(data, dto.CriadoEm);
    }
}

public class CreateVeiculoResponseTests
{
    [Fact]
    public void CreateVeiculoDto_QuandoCriado_DeveInicializarComValoresPadrao()
    {
        // Arrange & Act
        var dto = new CriarVeiculoRequest();

        // Assert
        Assert.Equal(Guid.Empty, dto.ClienteId);
        Assert.Empty(dto.Placa);
        Assert.Empty(dto.Marca);
        Assert.Empty(dto.Modelo);
        Assert.Equal(0, dto.Ano);
    }

    [Fact]
    public void CreateVeiculoDto_QuandoPopulado_DeveArmazenarValoresCorretamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var dto = new CriarVeiculoRequest
        {
            ClienteId = clienteId,
            Placa = "XYZ-9999",
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2024
        };

        // Act & Assert
        Assert.Equal(clienteId, dto.ClienteId);
        Assert.Equal("XYZ-9999", dto.Placa);
        Assert.Equal("Honda", dto.Marca);
        Assert.Equal("Civic", dto.Modelo);
        Assert.Equal(2024, dto.Ano);
    }

    [Fact]
    public void CreateVeiculoDto_ClienteIdObrigatorio_DeveSerGuidValido()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var dto = new CriarVeiculoRequest { ClienteId = clienteId };

        // Assert
        Assert.NotEqual(Guid.Empty, dto.ClienteId);
        Assert.Equal(clienteId, dto.ClienteId);
    }
}

public class UpdateVeiculoResponseTests
{
    [Fact]
    public void UpdateVeiculoDto_QuandoCriado_DeveInicializarComValoresPadrao()
    {
        // Arrange & Act
        var dto = new AtualizarVeiculoRequest();

        // Assert
        Assert.Null(dto.ClienteId);
        Assert.Empty(dto.Placa);
        Assert.Empty(dto.Marca);
        Assert.Empty(dto.Modelo);
        Assert.Equal(0, dto.Ano);
    }

    [Fact]
    public void UpdateVeiculoDto_QuandoPopulado_DeveArmazenarValoresCorretamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var dto = new AtualizarVeiculoRequest
        {
            ClienteId = clienteId,
            Placa = "ABC-1234",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023
        };

        // Act & Assert
        Assert.Equal(clienteId, dto.ClienteId);
        Assert.Equal("ABC-1234", dto.Placa);
        Assert.Equal("Toyota", dto.Marca);
        Assert.Equal("Corolla", dto.Modelo);
        Assert.Equal(2023, dto.Ano);
    }

    [Fact]
    public void UpdateVeiculoDto_ClienteIdOpcional_DevePermitirNulo()
    {
        // Arrange & Act
        var dto = new AtualizarVeiculoRequest { Placa = "XYZ-9999" };

        // Assert
        Assert.Null(dto.ClienteId);
        Assert.Equal("XYZ-9999", dto.Placa);
    }

    [Fact]
    public void UpdateVeiculoDto_DevePermitirAtualizacaoSeletivaDePropriedades()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var dto = new AtualizarVeiculoRequest { Placa = "ABC-1234" };

        // Act
        dto.ClienteId = clienteId;
        dto.Marca = "Ford";
        dto.Ano = 2022;

        // Assert
        Assert.Equal(clienteId, dto.ClienteId);
        Assert.Equal("ABC-1234", dto.Placa);
        Assert.Equal("Ford", dto.Marca);
        Assert.Empty(dto.Modelo);
        Assert.Equal(2022, dto.Ano);
    }
}
