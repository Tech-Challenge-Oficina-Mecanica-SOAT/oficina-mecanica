namespace OficinaMecanica.Application.DTOs;

public record HistoricoStatusOSDto(
    Guid Id,
    Guid OrdemServicoId,
    string? StatusAnterior,
    string StatusNovo,
    DateTime AlteradoEm,
    string AlteradoPor,
    string Motivo
);
