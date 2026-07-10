using System.Globalization;
using System.Text;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.API.Common;

/// <summary>
/// Monta a página HTML pública de acompanhamento de status de uma Ordem de Serviço.
/// Segue a mesma linguagem visual (gradientes, cards arredondados) já usada em
/// WebhookController para as páginas de aprovação/recusa de orçamento por e-mail.
/// </summary>
public static class PainelStatusHtmlBuilder
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static string Construir(OrdemServico os)
    {
        var (corPrincipal, corSecundaria, statusLabel) = ObterAparenciaStatus(os.StatusOS);
        var osIdCurto = os.Id.ToString().Substring(0, 8);
        var nomeCliente = os.Cliente?.Nome ?? "Cliente";
        var veiculoInfo = os.Veiculo != null
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} ({os.Veiculo.Ano}) - Placa {os.Veiculo.Placa?.Valor ?? "não informada"}"
            : "Não informado";

        var historicoOrdenado = os.Historico
            .OrderByDescending(h => h.AlteradoEm)
            .ToList();

        var ultimaAtualizacao = historicoOrdenado.FirstOrDefault()?.AlteradoEm
            ?? os.DataFechamento
            ?? os.DataAbertura;

        var timelineHtml = new StringBuilder();
        foreach (var h in historicoOrdenado)
        {
            var (corItem, _, labelItem) = ObterAparenciaStatus(h.StatusNovo);
            var motivoHtml = string.IsNullOrWhiteSpace(h.Motivo)
                ? string.Empty
                : $"<p class='timeline-motivo'>{h.Motivo}</p>";

            timelineHtml.Append($@"
                <div class='timeline-item'>
                    <div class='timeline-dot' style='background-color: {corItem};'></div>
                    <div class='timeline-content'>
                        <p class='timeline-status'>{labelItem}</p>
                        <p class='timeline-date'>{h.AlteradoEm.ToString("dd/MM/yyyy 'às' HH:mm", PtBr)}</p>
                        {motivoHtml}
                    </div>
                </div>");
        }

        return $@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Status da OS #{osIdCurto} - Oficina Mecânica</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f5f7;
            min-height: 100vh;
            display: flex;
            justify-content: center;
            padding: 30px 15px;
        }}
        .container {{
            max-width: 560px;
            width: 100%;
            background: white;
            border-radius: 16px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.08);
            overflow: hidden;
            height: fit-content;
        }}
        .header {{
            background: linear-gradient(135deg, {corPrincipal} 0%, {corSecundaria} 100%);
            padding: 32px 24px;
            text-align: center;
            color: white;
        }}
        .header p.eyebrow {{
            font-size: 12px;
            letter-spacing: 1px;
            text-transform: uppercase;
            opacity: 0.85;
            margin-bottom: 6px;
        }}
        .header h1 {{
            font-size: 22px;
            margin-bottom: 4px;
        }}
        .header p.subtitle {{
            font-size: 14px;
            opacity: 0.9;
        }}
        .status-badge {{
            display: inline-block;
            margin-top: 14px;
            padding: 6px 16px;
            border-radius: 20px;
            background: rgba(255,255,255,0.22);
            font-weight: 600;
            font-size: 14px;
        }}
        .content {{
            padding: 28px 24px;
        }}
        .info-box {{
            background-color: #f9f9f9;
            border: 1px solid #eee;
            border-radius: 10px;
            padding: 16px;
            margin-bottom: 24px;
        }}
        .info-box p {{
            font-size: 14px;
            color: #444;
            margin-bottom: 6px;
        }}
        .info-box p:last-child {{ margin-bottom: 0; }}
        .info-box strong {{ color: #111; }}
        .section-title {{
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            color: #888;
            margin-bottom: 14px;
        }}
        .timeline-item {{
            display: flex;
            gap: 12px;
            padding-bottom: 20px;
            position: relative;
        }}
        .timeline-item:not(:last-child)::before {{
            content: '';
            position: absolute;
            left: 5px;
            top: 16px;
            bottom: -4px;
            width: 2px;
            background: #e5e5e5;
        }}
        .timeline-dot {{
            width: 12px;
            height: 12px;
            border-radius: 50%;
            margin-top: 4px;
            flex-shrink: 0;
        }}
        .timeline-status {{ font-weight: 600; font-size: 14px; color: #222; }}
        .timeline-date {{ font-size: 12px; color: #999; margin-top: 2px; }}
        .timeline-motivo {{ font-size: 13px; color: #666; margin-top: 4px; }}
        .footer {{
            background: #f8f9fa;
            padding: 16px;
            text-align: center;
            font-size: 12px;
            color: #999;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <p class='eyebrow'>Ordem de Serviço #{osIdCurto}</p>
            <h1>Olá, {nomeCliente}</h1>
            <p class='subtitle'>Acompanhe abaixo o andamento do seu veículo</p>
            <div class='status-badge'>{statusLabel}</div>
        </div>
        <div class='content'>
            <div class='info-box'>
                <p><strong>Veículo:</strong> {veiculoInfo}</p>
                <p><strong>Aberta em:</strong> {os.DataAbertura.ToString("dd/MM/yyyy 'às' HH:mm", PtBr)}</p>
                <p><strong>Última atualização:</strong> {ultimaAtualizacao.ToString("dd/MM/yyyy 'às' HH:mm", PtBr)}</p>
            </div>
            <p class='section-title'>Histórico</p>
            <div class='timeline'>
                {timelineHtml}
            </div>
        </div>
        <div class='footer'>
            <p>Oficina Mecânica - Atendimento de qualidade<br/>
            Esta página é atualizada automaticamente conforme o andamento da OS.</p>
        </div>
    </div>
</body>
</html>";
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