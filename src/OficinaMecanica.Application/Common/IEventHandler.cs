using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.Common;

public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent evt);
}
