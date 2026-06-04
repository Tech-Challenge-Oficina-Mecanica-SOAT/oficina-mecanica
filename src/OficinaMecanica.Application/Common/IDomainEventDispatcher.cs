using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.Common;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events);
}
