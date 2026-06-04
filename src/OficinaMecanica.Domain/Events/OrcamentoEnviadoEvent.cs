using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Events;

public record OrcamentoEnviadoEvent(
    Guid OrdemServicoId,
    string EmailCliente,
    decimal Total,
    DateTime OcorridoEm) : IDomainEvent;
