using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.ForcarStatusOS;

public record ForcarStatusOSRequest(Guid OsId, EnumStatusOS NovoStatus, string AlteradoPor, string Motivo);
