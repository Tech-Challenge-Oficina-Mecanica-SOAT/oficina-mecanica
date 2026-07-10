using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.Infrastructure.Notifications;

public class EmailNotificacaoServiceFake : INotificacaoService
{
    private readonly IAppLogger<EmailNotificacaoServiceFake> _logger;

    public EmailNotificacaoServiceFake(IAppLogger<EmailNotificacaoServiceFake> logger)
    {
        _logger = logger;
    }

    public Task EnviarOrcamentoAsync(Guid osId, string emailCliente, decimal totalOrcamento)
    {
        _logger.Info($"FAKE - EnviarOrcamentoAsync: OS={osId}, Email={emailCliente}, Total={totalOrcamento}");
        return Task.CompletedTask;
    }

    public Task EnviarConclusaoAsync(Guid osId, string emailCliente)
    {
        _logger.Info($"FAKE - EnviarConclusaoAsync: OS={osId}, Email={emailCliente}");
        return Task.CompletedTask;
    }

    public Task EnviarAprovacaoAsync(Guid osId, string emailCliente)
    {
        _logger.Info($"FAKE - EnviarAprovacaoAsync: OS={osId}, Email={emailCliente}");
        return Task.CompletedTask;
    }

    public Task EnviarRejeicaoAsync(Guid osId, string emailCliente, string motivo)
    {
        _logger.Info($"FAKE - EnviarRejeicaoAsync: OS={osId}, Email={emailCliente}, Motivo={motivo}");
        return Task.CompletedTask;
    }

    public Task EnviarEntregaAsync(Guid osId, string emailCliente)
    {
        _logger.Info($"FAKE - EnviarEntregaAsync: OS={osId}, Email={emailCliente}");
        return Task.CompletedTask;
    }

    public Task EnviarDiagnosticoAsync(Guid osId, string emailCliente)
    {
        _logger.Info($"FAKE - EnviarDiagnosticoAsync: OS={osId}, Email={emailCliente}");
        return Task.CompletedTask;
    }

    public Task EnviarCriacaoAsync(Guid osId, string emailCliente)
    {
        _logger.Info($"FAKE - EnviarCriacaoAsync: OS={osId}, Email={emailCliente}");
        return Task.CompletedTask;
    }
}