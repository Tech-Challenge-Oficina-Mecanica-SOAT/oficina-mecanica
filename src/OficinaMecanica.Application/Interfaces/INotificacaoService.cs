namespace OficinaMecanica.Application.Interfaces;

public interface INotificacaoService
{
    Task EnviarOrcamentoAsync(Guid osId, string emailCliente, decimal totalOrcamento);
    Task EnviarAprovacaoAsync(Guid osId, string emailCliente);
    Task EnviarRejeicaoAsync(Guid osId, string emailCliente, string motivo);
    Task EnviarConclusaoAsync(Guid osId, string emailCliente, string motivo);
    Task EnviarEntregaAsync(Guid osId, string emailCliente);
}
