using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Interfaces;

public interface IOrdemServicoMetrics
{
    void RegistrarAbertura();
    void AtualizarStatus(EnumStatusOS? statusAnterior, EnumStatusOS statusNovo);
    void RegistrarTempoExecucao(TimeSpan duracao);
}
