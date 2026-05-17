using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _provider;

    public DomainEventDispatcher(IServiceProvider provider) => _provider = provider;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events)
    {
        foreach (var evt in events)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(evt.GetType());
            var handlers = _provider.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                if (handler is null) continue;
                var method = handlerType.GetMethod("HandleAsync");
                if (method is null) continue;
                var task = (Task)method.Invoke(handler, new object[] { evt })!;
                await task;
            }
        }
    }
}
