using System.Net;
using System.Text;

namespace OficinaMecanica.API.Presentation.PainelStatus;

/// <summary>
/// Renderiza a página HTML pública de acompanhamento de status de uma Ordem de Serviço.
/// Camada puramente de apresentação: recebe um <see cref="PainelStatusViewModel"/> já
/// pronto (sem regra de negócio) e apenas gera a marcação.
///
/// Responsabilidade única: renderização de HTML. Cada seção da página é montada por um
/// método próprio para facilitar leitura, manutenção e testes isolados.
///
/// Segurança: todo valor de origem do usuário/domínio (nome do cliente, dados do veículo,
/// motivo do histórico, etc.) passa por <see cref="WebUtility.HtmlEncode"/> antes de ser
/// inserido na marcação, evitando vulnerabilidades de XSS. Valores que não vêm de entrada
/// do usuário (cores hexadecimais fixas definidas internamente pela aplicação) não
/// precisam de encoding, pois nunca contêm dados externos.
/// </summary>
public class PainelStatusHtmlRenderer : IPainelStatusHtmlRenderer
{
    public string Renderizar(PainelStatusViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var html = new StringBuilder();
        html.Append("<!DOCTYPE html>");
        html.Append("<html lang='pt-BR'>");
        html.Append(RenderizarHead(viewModel));
        html.Append("<body>");
        html.Append("<div class='container'>");
        html.Append(RenderizarHeader(viewModel));
        html.Append(RenderizarConteudo(viewModel));
        html.Append(RenderizarFooter());
        html.Append("</div>");
        html.Append("</body>");
        html.Append("</html>");

        return html.ToString();
    }

    private static string RenderizarHead(PainelStatusViewModel vm)
    {
        var titulo = Enc($"Status da OS #{vm.OsIdCurto} - Oficina Mecânica");

        return $@"
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{titulo}</title>
    <style>{ObterEstilos()}</style>
</head>";
    }

    private static string RenderizarHeader(PainelStatusViewModel vm)
    {
        // Cores são geradas internamente pela aplicação (não vêm de entrada do usuário),
        // por isso podem ser inseridas diretamente no atributo style sem encoding de texto.
        var osIdCurto = Enc(vm.OsIdCurto);
        var nomeCliente = Enc(vm.NomeCliente);
        var statusLabel = Enc(vm.StatusLabel);

        return $@"
<div class='header' style='background: linear-gradient(135deg, {vm.CorPrincipal} 0%, {vm.CorSecundaria} 100%);'>
    <p class='eyebrow'>Ordem de Serviço #{osIdCurto}</p>
    <h1>Olá, {nomeCliente}</h1>
    <p class='subtitle'>Acompanhe abaixo o andamento do seu veículo</p>
    <div class='status-badge'>{statusLabel}</div>
</div>";
    }

    private static string RenderizarConteudo(PainelStatusViewModel vm)
    {
        return $@"
<div class='content'>
    {RenderizarInfoBox(vm)}
    <p class='section-title'>Histórico</p>
    <div class='timeline'>
        {RenderizarTimeline(vm)}
    </div>
</div>";
    }

    private static string RenderizarInfoBox(PainelStatusViewModel vm)
    {
        var veiculoInfo = Enc(vm.VeiculoInfo);
        var dataAbertura = Enc(vm.DataAberturaFormatada);
        var ultimaAtualizacao = Enc(vm.UltimaAtualizacaoFormatada);

        return $@"
<div class='info-box'>
    <p><strong>Veículo:</strong> {veiculoInfo}</p>
    <p><strong>Aberta em:</strong> {dataAbertura}</p>
    <p><strong>Última atualização:</strong> {ultimaAtualizacao}</p>
</div>";
    }

    private static string RenderizarTimeline(PainelStatusViewModel vm)
    {
        if (vm.Timeline.Count == 0)
            return string.Empty;

        var timelineHtml = new StringBuilder();
        foreach (var item in vm.Timeline)
            timelineHtml.Append(RenderizarTimelineItem(item));

        return timelineHtml.ToString();
    }

    private static string RenderizarTimelineItem(PainelStatusTimelineItem item)
    {
        var label = Enc(item.Label);
        var data = Enc(item.DataFormatada);
        var motivoHtml = string.IsNullOrWhiteSpace(item.Motivo)
            ? string.Empty
            : $"<p class='timeline-motivo'>{Enc(item.Motivo)}</p>";

        return $@"
<div class='timeline-item'>
    <div class='timeline-dot' style='background-color: {item.Cor};'></div>
    <div class='timeline-content'>
        <p class='timeline-status'>{label}</p>
        <p class='timeline-date'>{data}</p>
        {motivoHtml}
    </div>
</div>";
    }

    private static string RenderizarFooter() => @"
<div class='footer'>
    <p>Oficina Mecânica - Atendimento de qualidade<br/>
    Esta página é atualizada automaticamente conforme o andamento da OS.</p>
</div>";

    /// <summary>
    /// Escapa qualquer valor antes de inseri-lo no HTML, prevenindo XSS.
    /// </summary>
    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string ObterEstilos() => @"
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f5f7;
            min-height: 100vh;
            display: flex;
            justify-content: center;
            padding: 30px 15px;
        }
        .container {
            max-width: 560px;
            width: 100%;
            background: white;
            border-radius: 16px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.08);
            overflow: hidden;
            height: fit-content;
        }
        .header {
            padding: 32px 24px;
            text-align: center;
            color: white;
        }
        .header p.eyebrow {
            font-size: 12px;
            letter-spacing: 1px;
            text-transform: uppercase;
            opacity: 0.85;
            margin-bottom: 6px;
        }
        .header h1 {
            font-size: 22px;
            margin-bottom: 4px;
        }
        .header p.subtitle {
            font-size: 14px;
            opacity: 0.9;
        }
        .status-badge {
            display: inline-block;
            margin-top: 14px;
            padding: 6px 16px;
            border-radius: 20px;
            background: rgba(255,255,255,0.22);
            font-weight: 600;
            font-size: 14px;
        }
        .content {
            padding: 28px 24px;
        }
        .info-box {
            background-color: #f9f9f9;
            border: 1px solid #eee;
            border-radius: 10px;
            padding: 16px;
            margin-bottom: 24px;
        }
        .info-box p {
            font-size: 14px;
            color: #444;
            margin-bottom: 6px;
        }
        .info-box p:last-child { margin-bottom: 0; }
        .info-box strong { color: #111; }
        .section-title {
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            color: #888;
            margin-bottom: 14px;
        }
        .timeline-item {
            display: flex;
            gap: 12px;
            padding-bottom: 20px;
            position: relative;
        }
        .timeline-item:not(:last-child)::before {
            content: '';
            position: absolute;
            left: 5px;
            top: 16px;
            bottom: -4px;
            width: 2px;
            background: #e5e5e5;
        }
        .timeline-dot {
            width: 12px;
            height: 12px;
            border-radius: 50%;
            margin-top: 4px;
            flex-shrink: 0;
        }
        .timeline-status { font-weight: 600; font-size: 14px; color: #222; }
        .timeline-date { font-size: 12px; color: #999; margin-top: 2px; }
        .timeline-motivo { font-size: 13px; color: #666; margin-top: 4px; }
        .footer {
            background: #f8f9fa;
            padding: 16px;
            text-align: center;
            font-size: 12px;
            color: #999;
        }";
}
