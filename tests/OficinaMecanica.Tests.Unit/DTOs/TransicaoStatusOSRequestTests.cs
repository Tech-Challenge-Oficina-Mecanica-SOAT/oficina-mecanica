using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.DTOs;

public class TransicaoStatusOSRequestTests
{
    [Fact]
    public void Construtor_DeveC_CriarComParametrosValidos()
    {
        // Arrange
        var novoStatus = EnumStatusOS.EmExecucao;
        var motivo = "Diagnostico completo - Em Execução";

        // Act
        var dto = new TransicaoStatusOSRequest(novoStatus, motivo);

        // Assert
        Assert.Equal(novoStatus, dto.NovoStatus);
        Assert.Equal(motivo, dto.Motivo);
    }

    [Fact]
    public void Igualdade_DeveRetornarVerdadeiro_QuandoDtosTemMesmosDados()
    {
        // Arrange
        var novoStatus = EnumStatusOS.EmExecucao;
        var motivo = "Iniciando reparo";

        // Act
        var dto1 = new TransicaoStatusOSRequest(novoStatus, motivo);
        var dto2 = new TransicaoStatusOSRequest(novoStatus, motivo);

        // Assert
        Assert.Equal(dto1, dto2);
    }

    [Fact]
    public void Igualdade_DeveRetornarFalso_QuandoDtosTemDadosDiferentes()
    {
        // Arrange
        var dto1 = new TransicaoStatusOSRequest(EnumStatusOS.EmDiagnostico, "Diagnostico");
        var dto2 = new TransicaoStatusOSRequest(EnumStatusOS.EmExecucao, "Reparação");

        // Assert
        Assert.NotEqual(dto1, dto2);
    }

    [Fact]
    public void MotivoVazio_DeveSerPermitido()
    {
        // Arrange
        var novoStatus = EnumStatusOS.Finalizada;
        var motivo = string.Empty;

        // Act
        var dto = new TransicaoStatusOSRequest(novoStatus, motivo);

        // Assert
        Assert.Equal(string.Empty, dto.Motivo);
    }

    [Fact]
    public void Desconstrucao_DeveRetornarValoresCorretos()
    {
        // Arrange
        var novoStatus = EnumStatusOS.Rejeitada;
        var motivo = "Cliente solicitou cancelamento";
        var dto = new TransicaoStatusOSRequest(novoStatus, motivo);

        // Act
        var (status, motivoObtido) = dto;

        // Assert
        Assert.Equal(novoStatus, status);
        Assert.Equal(motivo, motivoObtido);
    }
}


