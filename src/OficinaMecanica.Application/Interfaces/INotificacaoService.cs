namespace OficinaMecanica.Application.Interfaces;

public interface INotificacaoService
{
    Task EnviarOrcamentoAsync(Guid osId, string emailCliente, decimal totalOrcamento);
    Task EnviarConclusaoAsync(Guid osId, string emailCliente);
}
