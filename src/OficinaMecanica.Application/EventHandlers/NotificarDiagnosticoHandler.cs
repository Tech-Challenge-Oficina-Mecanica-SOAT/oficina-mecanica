using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Application.EventHandlers;

public class NotificarDiagnosticoHandler : IEventHandler<DiagnosticoIniciadoEvent>
{
    private readonly INotificacaoService _notificacao;

    public NotificarDiagnosticoHandler(INotificacaoService notificacao) =>
        _notificacao = notificacao;

    public Task HandleAsync(DiagnosticoIniciadoEvent evt) =>
        _notificacao.EnviarDiagnosticoAsync(evt.OrdemServicoId, evt.EmailCliente);
}