using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Application.EventHandlers;

public class NotificarEntregaHandler : IEventHandler<OrdemEntregueEvent>
{
    private readonly INotificacaoService _notificacao;

    public NotificarEntregaHandler(INotificacaoService notificacao) =>
        _notificacao = notificacao;

    public Task HandleAsync(OrdemEntregueEvent evt) =>
        _notificacao.EnviarEntregaAsync(evt.OrdemServicoId, evt.EmailCliente);
}
