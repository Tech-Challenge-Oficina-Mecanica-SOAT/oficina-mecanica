using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Events;

public record OrdemAprovadaEvent(
    Guid OrdemServicoId,
    string EmailCliente,
    string AlteradoPor,
    DateTime OcorridoEm) : IDomainEvent;
