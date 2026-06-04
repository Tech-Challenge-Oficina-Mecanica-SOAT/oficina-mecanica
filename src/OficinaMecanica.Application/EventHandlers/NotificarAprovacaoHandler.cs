using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Application.EventHandlers;

public class NotificarAprovacaoHandler : IEventHandler<OrdemAprovadaEvent>
{
    private readonly INotificacaoService _notificacao;

    public NotificarAprovacaoHandler(INotificacaoService notificacao) =>
        _notificacao = notificacao;

    public Task HandleAsync(OrdemAprovadaEvent evt) =>
        _notificacao.EnviarAprovacaoAsync(evt.OrdemServicoId, evt.EmailCliente);
}
