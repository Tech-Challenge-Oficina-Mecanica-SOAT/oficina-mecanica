using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Events;

public record OrdemRejeitadaEvent(
    Guid OrdemServicoId,
    string EmailCliente,
    string AlteradoPor,
    string Motivo,
    DateTime OcorridoEm) : IDomainEvent;
