using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class HistoricoStatusOSMapper
{
    public HistoricoStatusOSResponse MapToResponse(HistoricoStatusOS h) => new(
        h.Id,
        h.OrdemServicoId,
        h.StatusAnterior?.ToString(),
        h.StatusNovo.ToString(),
        h.AlteradoEm,
        h.AlteradoPor,
        h.Motivo
    );
}
