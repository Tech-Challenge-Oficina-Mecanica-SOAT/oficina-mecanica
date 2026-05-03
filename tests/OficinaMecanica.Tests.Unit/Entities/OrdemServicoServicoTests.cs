using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.Entities;

public class OrdemServicoServicoTests
{
    [Fact]
    public void Constructor_ShouldCreateInstanceWithDefaultValues()
    {
        // Act
        var ordemServicoServico = new OrdemServicoServico();

        // Assert
        Assert.Equal(Guid.Empty, ordemServicoServico.OrdemServicoId);
        Assert.Equal(Guid.Empty, ordemServicoServico.ServicoId);
    }

    [Fact]
    public void OrdemServicoId_ShouldSetAndGetValue()
    {
        // Arrange
        var ordemServicoServico = new OrdemServicoServico();
        var ordemServicoId = Guid.NewGuid();

        // Act
        ordemServicoServico.OrdemServicoId = ordemServicoId;

        // Assert
        Assert.Equal(ordemServicoId, ordemServicoServico.OrdemServicoId);
    }

    [Fact]
    public void ServicoId_ShouldSetAndGetValue()
    {
        // Arrange
        var ordemServicoServico = new OrdemServicoServico();
        var servicoId = Guid.NewGuid();

        // Act
        ordemServicoServico.ServicoId = servicoId;

        // Assert
        Assert.Equal(servicoId, ordemServicoServico.ServicoId);
    }

    [Fact]
    public void OrdemServico_ShouldSetAndGetValue()
    {
        // Arrange
        var ordemServicoServico = new OrdemServicoServico();
        var ordemServico = new OrdemServico();

        // Act
        ordemServicoServico.OrdemServico = ordemServico;

        // Assert
        Assert.Equal(ordemServico, ordemServicoServico.OrdemServico);
    }

    [Fact]
    public void Servico_ShouldSetAndGetValue()
    {
        // Arrange
        var ordemServicoServico = new OrdemServicoServico();
        var servico = new Servico();

        // Act
        ordemServicoServico.Servico = servico;

        // Assert
        Assert.Equal(servico, ordemServicoServico.Servico);
    }

    [Fact]
    public void AllProperties_ShouldBeSetCorrectly()
    {
        // Arrange
        var ordemServicoId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var ordemServico = new OrdemServico();
        var servico = new Servico();

        // Act
        var ordemServicoServico = new OrdemServicoServico
        {
            OrdemServicoId = ordemServicoId,
            OrdemServico = ordemServico,
            ServicoId = servicoId,
            Servico = servico
        };

        // Assert
        Assert.Equal(ordemServicoId, ordemServicoServico.OrdemServicoId);
        Assert.Equal(ordemServico, ordemServicoServico.OrdemServico);
        Assert.Equal(servicoId, ordemServicoServico.ServicoId);
        Assert.Equal(servico, ordemServicoServico.Servico);
    }
}