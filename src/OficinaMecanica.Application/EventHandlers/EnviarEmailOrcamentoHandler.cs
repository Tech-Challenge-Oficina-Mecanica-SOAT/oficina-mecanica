using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Application.EventHandlers;

public class EnviarEmailOrcamentoHandler : IEventHandler<OrcamentoEnviadoEvent>
{
    private readonly INotificacaoService _notificacao;

    public EnviarEmailOrcamentoHandler(INotificacaoService notificacao) =>
        _notificacao = notificacao;

    public Task HandleAsync(OrcamentoEnviadoEvent evt) =>
        _notificacao.EnviarOrcamentoAsync(evt.OrdemServicoId, evt.EmailCliente, evt.Total);
}
