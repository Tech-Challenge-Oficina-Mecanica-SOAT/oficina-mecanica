using FluentAssertions;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Tests.Unit.DTOs;

public class HistoricoStatusOSResponseTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var statusAnterior = "Pendente";
        var statusNovo = "Em Progresso";
        var alteradoEm = DateTime.Now;
        var alteradoPor = "usuario@teste.com";
        var motivo = "Iniciou o trabalho";

        // Act
        var dto = new HistoricoStatusOSResponse(id, ordemServicoId, statusAnterior, statusNovo, alteradoEm, alteradoPor, motivo);

        // Assert
        dto.Id.Should().Be(id);
        dto.OrdemServicoId.Should().Be(ordemServicoId);
        dto.StatusAnterior.Should().Be(statusAnterior);
        dto.StatusNovo.Should().Be(statusNovo);
        dto.AlteradoEm.Should().Be(alteradoEm);
        dto.AlteradoPor.Should().Be(alteradoPor);
        dto.Motivo.Should().Be(motivo);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithNullStatusAnterior()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();

        // Act
        var dto = new HistoricoStatusOSResponse(id, ordemServicoId, null, "Pendente", DateTime.Now, "usuario", "Motivo");

        // Assert
        dto.StatusAnterior.Should().BeNull();
        dto.StatusNovo.Should().NotBeNull();
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForRecordsWithSameData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var alteradoEm = DateTime.Now;

        var dto1 = new HistoricoStatusOSResponse(id, ordemServicoId, "Pendente", "Em Progresso", alteradoEm, "usuario", "Motivo");
        var dto2 = new HistoricoStatusOSResponse(id, ordemServicoId, "Pendente", "Em Progresso", alteradoEm, "usuario", "Motivo");

        // Act & Assert
        dto1.Should().Be(dto2);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_ForRecordsWithDifferentData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var alteradoEm = DateTime.Now;

        var dto1 = new HistoricoStatusOSResponse(id, ordemServicoId, "Pendente", "Em Progresso", alteradoEm, "usuario1", "Motivo");
        var dto2 = new HistoricoStatusOSResponse(id, ordemServicoId, "Pendente", "Concluído", alteradoEm, "usuario2", "Motivo");

        // Act & Assert
        dto1.Should().NotBe(dto2);
    }

    [Fact]
    public void Deconstruction_ShouldReturnAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var statusAnterior = "Pendente";
        var statusNovo = "Em Progresso";
        var alteradoEm = DateTime.Now;
        var alteradoPor = "usuario@teste.com";
        var motivo = "Iniciou o trabalho";

        var dto = new HistoricoStatusOSResponse(id, ordemServicoId, statusAnterior, statusNovo, alteradoEm, alteradoPor, motivo);

        // Act
        var (dtoId, dtoOrdemServicoId, dtoStatusAnterior, dtoStatusNovo, dtoAlteradoEm, dtoAlteradoPor, dtoMotivo) = dto;

        // Assert
        dtoId.Should().Be(id);
        dtoOrdemServicoId.Should().Be(ordemServicoId);
        dtoStatusAnterior.Should().Be(statusAnterior);
        dtoStatusNovo.Should().Be(statusNovo);
        dtoAlteradoEm.Should().Be(alteradoEm);
        dtoAlteradoPor.Should().Be(alteradoPor);
        dtoMotivo.Should().Be(motivo);
    }
}