using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Events;

public record OrdemCriadaEvent(
    Guid OrdemServicoId,
    string EmailCliente,
    DateTime OcorridoEm) : IDomainEvent;