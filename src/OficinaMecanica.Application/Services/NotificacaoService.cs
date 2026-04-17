using Microsoft.Extensions.Logging;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.Application.Services;

public class NotificacaoService : INotificacaoService
{
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(ILogger<NotificacaoService> logger) => _logger = logger;

    public Task EnviarOrcamentoAsync(Guid osId, string emailCliente, decimal totalOrcamento)
    {
        _logger.LogInformation(
            "Orcamento enviado. OS: {OsId}, Cliente: {Email}, Total: {Total}",
            osId, emailCliente, totalOrcamento);

        return Task.CompletedTask;
    }
}
