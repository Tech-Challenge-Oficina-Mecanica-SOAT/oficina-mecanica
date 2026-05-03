using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.Entities;

public class VeiculoTests
{
    private readonly Guid _clienteIdValido = Guid.NewGuid();

    #region Constructor

    [Fact]
    public void Constructor_ComDadosValidos_DevecriarVeiculo()
    {
        // Arrange
        var placa = "ABC1234";
        var marca = "Toyota";
        var modelo = "Corolla";
        var ano = 2023;

        // Act
        var veiculo = new Veiculo(_clienteIdValido, placa, marca, modelo, ano);

        // Assert
        Assert.NotEqual(Guid.Empty, veiculo.Id);
        Assert.Equal("ABC1234", veiculo.Placa);
        Assert.Equal(marca, veiculo.Marca);
        Assert.Equal(modelo, veiculo.Modelo);
        Assert.Equal(ano, veiculo.Ano);
        Assert.Equal(_clienteIdValido, veiculo.ClienteId);
        Assert.NotEqual(default, veiculo.CriadoEm);
    }

    [Fact]
    public void Constructor_ComClienteIdVazio_DevelancarException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Veiculo(Guid.Empty, "ABC1234", "Toyota", "Corolla", 2023));

        Assert.Equal("ClienteId é obrigatório", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABCD1234")]
    [InlineData("AB11234")]
    [InlineData("ABC")]
    [InlineData("1234567")]
    [InlineData("ABC12D4")]
    public void Constructor_ComPlacaInvalida_DevelancarException(string placa)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Veiculo(_clienteIdValido, placa, "Toyota", "Corolla", 2023));

        Assert.Equal("Placa inválida. Formato: ABC1234 ou ABC1D23", ex.Message);
    }

    [Theory]
    [InlineData("abc1234")]
    [InlineData("ABC 1234")]
    [InlineData("ABC-1234")]
    [InlineData("ABC 1 D 23")]
    public void Constructor_ComPlacaValidaMasFormatada_DeveNormalizarecriarVeiculo(string placa)
    {
        // Act
        var veiculo = new Veiculo(_clienteIdValido, placa, "Toyota", "Corolla", 2023);

        // Assert
        Assert.NotNull(veiculo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_ComMarcaInvalida_DevelancarException(string marca)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Veiculo(_clienteIdValido, "ABC1234", marca, "Corolla", 2023));

        Assert.Equal("Marca é obrigatória", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_ComModeloInvalido_DevelancarException(string modelo)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Veiculo(_clienteIdValido, "ABC1234", "Toyota", modelo, 2023));

        Assert.Equal("Modelo é obrigatório", ex.Message);
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(1000)]
    public void Constructor_ComAnoMuitoAntigo_DevelancarException(int ano)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Veiculo(_clienteIdValido, "ABC1234", "Toyota", "Corolla", ano));

        Assert.Equal("Ano inválido", ex.Message);
    }

    [Fact]
    public void Constructor_ComAnoFuturoPermitido_DevecriarVeiculo()
    {
        // Arrange
        var anoFuturo = DateTime.Now.Year + 1;

        // Act
        var veiculo = new Veiculo(_clienteIdValido, "ABC1234", "Toyota", "Corolla", anoFuturo);

        // Assert
        Assert.Equal(anoFuturo, veiculo.Ano);
    }

    [Fact]
    public void Constructor_ComAnoMuitoFuturo_DevelancarException()
    {
        // Arrange
        var anoInvalido = DateTime.Now.Year + 2;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Veiculo(_clienteIdValido, "ABC1234", "Toyota", "Corolla", anoInvalido));

        Assert.Equal("Ano inválido", ex.Message);
    }

    #endregion

    #region Atualizar

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarVeiculo()
    {
        // Arrange
        var veiculo = new Veiculo(_clienteIdValido, "ABC1234", "Toyota", "Corolla", 2023);
        var novoClienteId = Guid.NewGuid();
        var novaPlaca = "XYZ5678";
        var novaMarca = "Honda";
        var novoModelo = "Civic";
        var novoAno = 2024;

        // Act
        veiculo.Atualizar(novoClienteId, novaPlaca, novaMarca, novoModelo, novoAno);

        // Assert
        Assert.Equal(novoClienteId, veiculo.ClienteId);
        Assert.Equal("XYZ5678", veiculo.Placa);
        Assert.Equal(novaMarca, veiculo.Marca);
        Assert.Equal(novoModelo, veiculo.Modelo);
        Assert.Equal(novoAno, veiculo.Ano);
    }

    [Fact]
    public void Atualizar_ComClienteIdNulo_NaoDeveAlterarClienteId()
    {
        // Arrange
        var veiculo = new Veiculo(_clienteIdValido, "ABC1234", "Toyota", "Corolla", 2023);

        // Act
        veiculo.Atualizar(null, "XYZ5678", "Honda", "Civic", 2024);

        // Assert
        Assert.Equal(_clienteIdValido, veiculo.ClienteId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABCD1234")]
    [InlineData("ABC12D4")]
    public void Atualizar_ComPlacaInvalida_DevelancarException(string placa)
    {
        // Arrange
        var veiculo = new Veiculo(_clienteIdValido, "ABC1234", "Toyota", "Corolla", 2023);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            veiculo.Atualizar(null, placa, "Honda", "Civic", 2024));

        Assert.Equal("Placa inválida. Formato: ABC1234 ou ABC1D23", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Atualizar_ComMarcaInvalida_DevelancarException(string marca)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Veiculo(_clienteIdValido, "ABC1234", marca, "Corolla", 2023));

        Assert.Equal("Marca é obrigatória", ex.Message);
    }

    #endregion

    #region Validar Placa - Formatos

    [Theory]
    [InlineData("ABC1234")]
    [InlineData("XYZ9876")]
    [InlineData("aaa1111")]
    [InlineData("ABC 1234")]
    [InlineData("ABC-1234")]
    public void ValidarPlaca_ComFormatoAntigoValido_DeveRetornarTrue(string placa)
    {
        // Act
        var veiculo = new Veiculo(_clienteIdValido, placa, "Toyota", "Corolla", 2023);

        // Assert
        Assert.NotNull(veiculo);
    }

    [Theory]
    [InlineData("ABC1D23")]
    [InlineData("XYZ9A87")]
    [InlineData("aaa1a11")]
    [InlineData("ABC 1 D 23")]
    [InlineData("ABC-1-D-23")]
    public void ValidarPlaca_ComFormatoMercosulValido_DeveRetornarTrue(string placa)
    {
        // Act
        var veiculo = new Veiculo(_clienteIdValido, placa, "Toyota", "Corolla", 2023);

        // Assert
        Assert.NotNull(veiculo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("ABCD1234")]
    [InlineData("AB11234")]
    [InlineData("ABC123")]
    [InlineData("1234567")]
    [InlineData("ABC1DD3")]
    [InlineData("123ABCD")]
    public void ValidarPlaca_ComFormatoInvalido_DeveRetornarFalse(string placa)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Veiculo(_clienteIdValido, placa, "Toyota", "Corolla", 2023));

        Assert.Equal("Placa inválida. Formato: ABC1234 ou ABC1D23", ex.Message);
    }

    #endregion

    #region Normalizar Placa

    [Theory]
    [InlineData("abc1234", "ABC1234")]
    [InlineData("ABC 1234", "ABC1234")]
    [InlineData("ABC-1234", "ABC1234")]
    [InlineData("a b c 1 2 3 4", "ABC1234")]
    [InlineData("a-b-c-1-2-3-4", "ABC1234")]
    public void NormalizarPlaca_DeveRemoverEspacosETracos(string placaOriginal, string placaEsperada)
    {
        // Act
        var veiculo = new Veiculo(_clienteIdValido, placaOriginal, "Toyota", "Corolla", 2023);

        // Assert
        Assert.Equal(placaEsperada, veiculo.Placa);
    }

    #endregion

    #region Colecoes

    [Fact]
    public void Constructor_DeveInicializarOrdensServicoVazia()
    {
        // Act
        var veiculo = new Veiculo(_clienteIdValido, "ABC1234", "Toyota", "Corolla", 2023);

        // Assert
        Assert.NotNull(veiculo.OrdensServico);
        Assert.Empty(veiculo.OrdensServico);
    }

    #endregion
}