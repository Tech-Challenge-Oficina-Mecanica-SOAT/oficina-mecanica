namespace OficinaMecanica.Application.DTOs.Responses;
public record HistoricoStatusOSResponse(
    Guid Id,
    Guid OrdemServicoId,
    string? StatusAnterior,
    string StatusNovo,
    DateTime AlteradoEm,
    string AlteradoPor,
    string? Motivo
);
