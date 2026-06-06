using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Application.EventHandlers;

public class NotificarOrcamentoHandler : IEventHandler<OrcamentoEnviadoEvent>
{
    private readonly INotificacaoService _notificacao;
    private static readonly HashSet<Guid> _processando = new();

    public NotificarOrcamentoHandler(INotificacaoService notificacao) =>
        _notificacao = notificacao;

    public async Task HandleAsync(OrcamentoEnviadoEvent evt)
    {
        if (_processando.Contains(evt.OrdemServicoId))
            return;
        
        _processando.Add(evt.OrdemServicoId);
        
        try
        {
            await _notificacao.EnviarOrcamentoAsync(evt.OrdemServicoId, evt.EmailCliente, evt.Total);
        }
        finally
        {
            _processando.Remove(evt.OrdemServicoId);
        }
    }
}