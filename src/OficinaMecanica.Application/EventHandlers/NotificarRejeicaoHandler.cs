using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Application.EventHandlers;

public class NotificarRejeicaoHandler : IEventHandler<OrdemRejeitadaEvent>
{
    private readonly INotificacaoService _notificacao;

    public NotificarRejeicaoHandler(INotificacaoService notificacao) =>
        _notificacao = notificacao;

    public Task HandleAsync(OrdemRejeitadaEvent evt) =>
        _notificacao.EnviarRejeicaoAsync(evt.OrdemServicoId, evt.EmailCliente, evt.Motivo);
}
