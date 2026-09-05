using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider provider, ILogger<DomainEventDispatcher> logger)
    {
        _provider = provider;
        _logger = logger;
    }

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

                // Handlers de evento sao efeito colateral (notificacao, metrica, etc).
                // A operacao principal ja foi persistida antes do dispatch (ver
                // ApplicationDbContext.SaveChangesAsync); uma falha aqui (ex: SMTP
                // fora do ar) nao pode derrubar uma requisicao que ja teve sucesso.
                try
                {
                    var task = (Task)method.Invoke(handler, new object[] { evt })!;
                    await task;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao processar {Handler} para o evento {Evento}",
                        handler.GetType().Name, evt.GetType().Name);
                }
            }
        }
    }
}
