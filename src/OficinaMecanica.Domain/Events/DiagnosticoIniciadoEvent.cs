using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Events;

public record DiagnosticoIniciadoEvent(
    Guid OrdemServicoId,
    string EmailCliente,
    string AlteradoPor,
    DateTime OcorridoEm) : IDomainEvent;