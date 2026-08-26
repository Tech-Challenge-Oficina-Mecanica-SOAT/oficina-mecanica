using System.Globalization;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.API.Presentation.PainelStatus;

/// <summary>
/// Constrói o <see cref="PainelStatusViewModel"/> a partir de uma <see cref="OrdemServico"/>.
/// Responsabilidade única: traduzir o modelo de domínio para o modelo de apresentação
/// (formatação de datas, rótulos e cores). Não conhece HTML.
/// </summary>
public class PainelStatusViewModelFactory : IPainelStatusViewModelFactory
{
    private static readonly CultureInfo PtBr = new("pt-BR");
    private const string FormatoData = "dd/MM/yyyy 'às' HH:mm";

    public PainelStatusViewModel CriarViewModel(OrdemServico os)
    {
        ArgumentNullException.ThrowIfNull(os);

        var (corPrincipal, corSecundaria, statusLabel) = ObterAparenciaStatus(os.StatusOS);

        var historicoOrdenado = os.Historico
            .OrderByDescending(h => h.AlteradoEm)
            .ToList();

        var ultimaAtualizacao = historicoOrdenado.FirstOrDefault()?.AlteradoEm
            ?? os.DataFechamento
            ?? os.DataAbertura;

        return new PainelStatusViewModel(
            OsIdCurto: os.Id.ToString()[..8],
            NomeCliente: os.Cliente?.Nome ?? "Cliente",
            VeiculoInfo: ObterVeiculoInfo(os.Veiculo),
            StatusLabel: statusLabel,
            CorPrincipal: corPrincipal,
            CorSecundaria: corSecundaria,
            DataAberturaFormatada: os.DataAbertura.ToString(FormatoData, PtBr),
            UltimaAtualizacaoFormatada: ultimaAtualizacao.ToString(FormatoData, PtBr),
            Timeline: historicoOrdenado.Select(MapearItemTimeline).ToList()
        );
    }

    private static PainelStatusTimelineItem MapearItemTimeline(HistoricoStatusOS h)
    {
        var (corItem, _, labelItem) = ObterAparenciaStatus(h.StatusNovo);

        return new PainelStatusTimelineItem(
            Cor: corItem,
            Label: labelItem,
            DataFormatada: h.AlteradoEm.ToString(FormatoData, PtBr),
            Motivo: string.IsNullOrWhiteSpace(h.Motivo) ? null : h.Motivo
        );
    }

    private static string ObterVeiculoInfo(Veiculo? veiculo)
    {
        if (veiculo is null)
            return "Não informado";

        var placa = veiculo.Placa?.Valor ?? "não informada";
        return $"{veiculo.Marca} {veiculo.Modelo} ({veiculo.Ano}) - Placa {placa}";
    }

    private static (string CorPrincipal, string CorSecundaria, string Label) ObterAparenciaStatus(EnumStatusOS status) => status switch
    {
        EnumStatusOS.Recebida => ("#4facfe", "#00c6ff", "Recebida"),
        EnumStatusOS.EmDiagnostico => ("#17a2b8", "#0f6674", "Em Diagnóstico"),
        EnumStatusOS.AguardandoAprovacao => ("#f7971e", "#f4a83f", "Aguardando Aprovação"),
        EnumStatusOS.EmExecucao => ("#667eea", "#764ba2", "Em Execução"),
        EnumStatusOS.Finalizada => ("#11998e", "#38ef7d", "Finalizada"),
        EnumStatusOS.Entregue => ("#11998e", "#38ef7d", "Entregue"),
        EnumStatusOS.Rejeitada => ("#eb3349", "#f45c43", "Rejeitada"),
        _ => ("#6c757d", "#495057", status.ToString())
    };
}
