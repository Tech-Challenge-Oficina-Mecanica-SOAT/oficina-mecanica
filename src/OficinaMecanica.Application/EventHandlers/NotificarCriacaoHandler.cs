using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.EventHandlers;

public class NotificarCriacaoHandler : IEventHandler<OrdemCriadaEvent>
{
    private readonly INotificacaoService _notificacao;
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    
    public NotificarCriacaoHandler(
        INotificacaoService notificacao,
        IOrdemServicoRepository ordemServicoRepository) 
    {
        _notificacao = notificacao;
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task HandleAsync(OrdemCriadaEvent evt)
    {
        var os = await _ordemServicoRepository.ObterPorIdComItensAsync(evt.OrdemServicoId);
        var email = os?.Cliente?.Email?.Valor ?? evt.EmailCliente;
        
        if (string.IsNullOrEmpty(email))
        {
            return;
        }
        
        await _notificacao.EnviarCriacaoAsync(evt.OrdemServicoId, email);
    }
}