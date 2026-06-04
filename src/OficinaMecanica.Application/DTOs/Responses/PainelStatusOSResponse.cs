namespace OficinaMecanica.Application.DTOs.Responses;
public record PainelStatusOSResponse(
    Guid OsId,
    string Status,
    DateTime AtualizadoEm
);
