using System.Text.RegularExpressions;
using Serilog.Events;
using Serilog.Formatting;

namespace OficinaMecanica.Infrastructure.Logging;

public class CpfMaskingTextFormatter : ITextFormatter
{
    private readonly ITextFormatter _inner;

    private static readonly Regex CpfFormatado =
        new(@"\b(\d{3})\.(\d{3})\.(\d{3})-(\d{2})\b", RegexOptions.Compiled);

    private static readonly Regex CpfNumerico =
        new(@"\b(\d{3})(\d{3})(\d{3})(\d{2})\b", RegexOptions.Compiled);

    public CpfMaskingTextFormatter(ITextFormatter inner) => _inner = inner;

    public void Format(LogEvent logEvent, TextWriter output)
    {
        using var buffer = new StringWriter();
        _inner.Format(logEvent, buffer);

        var texto = buffer.ToString();
        texto = CpfFormatado.Replace(texto, "$1.***.***-$4");
        texto = CpfNumerico.Replace(texto, "$1***$4");

        output.Write(texto);
    }
}
