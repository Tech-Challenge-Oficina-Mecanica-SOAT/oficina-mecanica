using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using Prometheus;

namespace OficinaMecanica.Infrastructure.Metrics;

public class OrdemServicoMetrics : IOrdemServicoMetrics
{
    private static readonly Counter OsAbertasTotal = Prometheus.Metrics.CreateCounter(
        "os_abertas_total", "Total de ordens de serviço abertas.");

    private static readonly Gauge OsPorStatusGauge = Prometheus.Metrics.CreateGauge(
        "os_por_status_gauge", "Quantidade atual de ordens de serviço por status.",
        new GaugeConfiguration { LabelNames = ["status"] });

    private static readonly Histogram TempoExecucaoHistogram = Prometheus.Metrics.CreateHistogram(
        "tempo_execucao_histogram", "Tempo de execução da OS da abertura até a entrega, em horas.");

    public void RegistrarAbertura()
    {
        OsAbertasTotal.Inc();
        OsPorStatusGauge.WithLabels(EnumStatusOS.Recebida.ToString()).Inc();
    }

    public void AtualizarStatus(EnumStatusOS? statusAnterior, EnumStatusOS statusNovo)
    {
        if (statusAnterior.HasValue)
            OsPorStatusGauge.WithLabels(statusAnterior.Value.ToString()).Dec();

        OsPorStatusGauge.WithLabels(statusNovo.ToString()).Inc();
    }

    public void RegistrarTempoExecucao(TimeSpan duracao) =>
        TempoExecucaoHistogram.Observe(duracao.TotalHours);
}
