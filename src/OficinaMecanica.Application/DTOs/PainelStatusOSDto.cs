namespace OficinaMecanica.Application.DTOs;

public record PainelStatusOSDto(
    Guid OsId,
    string Status,
    DateTime AtualizadoEm
);
