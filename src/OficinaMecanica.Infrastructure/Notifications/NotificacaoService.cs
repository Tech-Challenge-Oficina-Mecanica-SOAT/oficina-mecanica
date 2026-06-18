using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.Infrastructure.Notifications;

public class NotificacaoService : INotificacaoService
{
    private readonly IAppLogger<NotificacaoService> _logger;

    public NotificacaoService(IAppLogger<NotificacaoService> logger) => _logger = logger;

    public Task EnviarOrcamentoAsync(Guid osId, string emailCliente, decimal totalOrcamento)
    {
        _logger.Info(
            "Orcamento enviado. OS: {OsId}, Cliente: {Email}, Total: {Total}",
            osId, emailCliente, totalOrcamento);
        return Task.CompletedTask;
    }

    public Task EnviarAprovacaoAsync(Guid osId, string emailCliente)
    {
        _logger.Info(
            "Aprovacao notificada. OS: {OsId}, Cliente: {Email}",
            osId, emailCliente);
        return Task.CompletedTask;
    }

    public Task EnviarRejeicaoAsync(Guid osId, string emailCliente, string motivo)
    {
        _logger.Info(
            "Rejeicao notificada. OS: {OsId}, Cliente: {Email}, Motivo: {Motivo}",
            osId, emailCliente, motivo);
        return Task.CompletedTask;
    }

    public Task EnviarConclusaoAsync(Guid osId, string emailCliente, string motivo)
    {
        _logger.Info(
            "Notificacao de conclusao enviada. OS: {OsId}, Cliente: {Email}, Motivo: {Motivo}",
            osId, emailCliente, motivo);
        return Task.CompletedTask;
    }

    public Task EnviarEntregaAsync(Guid osId, string emailCliente)
    {
        _logger.Info(
            "Entrega notificada. OS: {OsId}, Cliente: {Email}",
            osId, emailCliente);
        return Task.CompletedTask;
    }
}
