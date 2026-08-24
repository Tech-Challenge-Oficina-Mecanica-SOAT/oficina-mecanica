using FluentAssertions;
using OficinaMecanica.Infrastructure.Logging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Parsing;

namespace OficinaMecanica.Tests.Unit.Logging;

public class CpfMaskingTextFormatterTests
{
    private sealed class FakeFormatter : ITextFormatter
    {
        private readonly string _texto;
        public FakeFormatter(string texto) => _texto = texto;
        public void Format(LogEvent logEvent, TextWriter output) => output.Write(_texto);
    }

    private static LogEvent CriarLogEvent()
    {
        var template = new MessageTemplateParser().Parse(string.Empty);
        return new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, null, template, []);
    }

    private static string Formatar(string entrada)
    {
        var sut = new CpfMaskingTextFormatter(new FakeFormatter(entrada));
        var writer = new StringWriter();
        sut.Format(CriarLogEvent(), writer);
        return writer.ToString();
    }

    [Fact]
    public void Format_ComCpfFormatado_Mascara()
    {
        var resultado = Formatar("Cliente CPF: 529.982.247-25");

        resultado.Should().Be("Cliente CPF: 529.***.***-25");
        resultado.Should().NotContain("982");
        resultado.Should().NotContain("247");
    }

    [Fact]
    public void Format_ComCpfNumerico_Mascara()
    {
        var resultado = Formatar("cpf=52998224725");

        resultado.Should().Be("cpf=529***25");
        resultado.Should().NotContain("52998224725");
    }

    [Fact]
    public void Format_SemCpf_NaoAltera()
    {
        var resultado = Formatar("mensagem sem dados sensiveis");

        resultado.Should().Be("mensagem sem dados sensiveis");
    }
}
