using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Application.EventHandlers;

public class NotificarConclusaoHandler : IEventHandler<OrdemConcluidaEvent>
{
    private readonly INotificacaoService _notificacao;

    public NotificarConclusaoHandler(INotificacaoService notificacao) =>
        _notificacao = notificacao;

    public Task HandleAsync(OrdemConcluidaEvent evt) =>
        _notificacao.EnviarConclusaoAsync(evt.OrdemServicoId, evt.EmailCliente, evt.Motivo);
}
