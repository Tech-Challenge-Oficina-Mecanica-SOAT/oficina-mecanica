using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Application.EventHandlers;

public class NotificarOrcamentoHandler : IEventHandler<OrcamentoEnviadoEvent>
{
    private readonly INotificacaoService _notificacao;

    public NotificarOrcamentoHandler(INotificacaoService notificacao) =>
        _notificacao = notificacao;

    public Task HandleAsync(OrcamentoEnviadoEvent evt) =>
        _notificacao.EnviarOrcamentoAsync(evt.OrdemServicoId, evt.EmailCliente, evt.Total);
}
