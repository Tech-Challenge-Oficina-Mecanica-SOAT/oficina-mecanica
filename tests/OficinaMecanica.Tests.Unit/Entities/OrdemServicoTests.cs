using FluentAssertions;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Unit.Entities;

public class OrdemServicoTests
{
    private static OrdemServico NovaOS() =>
        new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");

    // ----- CONSTRUTOR / INVARIANTES -----

    [Fact]
    public void Construtor_ComDadosValidos_CriaComStatusRecebida()
    {
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();

        var os = new OrdemServico(clienteId, veiculoId, "obs inicial");

        os.Id.Should().NotBe(Guid.Empty);
        os.ClienteId.Should().Be(clienteId);
        os.VeiculoId.Should().Be(veiculoId);
        os.StatusOS.Should().Be(EnumStatusOS.Recebida);
        os.Observacoes.Should().Be("obs inicial");
        os.DataAbertura.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        os.DataFechamento.Should().BeNull();
    }

    [Fact]
    public void Construtor_ComClienteIdVazio_LancaExcecao()
    {
        Action act = () => new OrdemServico(Guid.Empty, Guid.NewGuid(), "obs");
        act.Should().Throw<ArgumentException>().WithMessage("*Cliente*");
    }

    [Fact]
    public void Construtor_ComVeiculoIdVazio_LancaExcecao()
    {
        Action act = () => new OrdemServico(Guid.NewGuid(), Guid.Empty, "obs");
        act.Should().Throw<ArgumentException>().WithMessage("*Veiculo*");
    }

    [Fact]
    public void Construtor_RegistraTransicaoInicialNoHistorico()
    {
        var os = NovaOS();

        os.Historico.Should().HaveCount(1);
        os.Historico.Single().StatusAnterior.Should().BeNull();
        os.Historico.Single().StatusNovo.Should().Be(EnumStatusOS.Recebida);
    }

    // ----- TRANSIÇÕES VÁLIDAS -----

    [Fact]
    public void IniciarDiagnostico_DeRecebida_TransitaParaEmDiagnostico()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("mecanico1");

        os.StatusOS.Should().Be(EnumStatusOS.EmDiagnostico);
        os.Historico.Should().HaveCount(2);
        os.Historico.Last().StatusAnterior.Should().Be(EnumStatusOS.Recebida);
        os.Historico.Last().StatusNovo.Should().Be(EnumStatusOS.EmDiagnostico);
        os.Historico.Last().AlteradoPor.Should().Be("mecanico1");
    }

    [Fact]
    public void EnviarParaAprovacao_DeEmDiagnostico_TransitaParaAguardandoAprovacao()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("mec");
        os.EnviarParaAprovacao("M4-envio-orcamento");

        os.StatusOS.Should().Be(EnumStatusOS.AguardandoAprovacao);
    }

    [Fact]
    public void Aprovar_DeAguardandoAprovacao_TransitaParaEmExecucao()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("mec");
        os.EnviarParaAprovacao("M4");
        os.Aprovar("cliente");

        os.StatusOS.Should().Be(EnumStatusOS.EmExecucao);
    }

    [Fact]
    public void Rejeitar_DeAguardandoAprovacao_TransitaParaRejeitada()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("mec");
        os.EnviarParaAprovacao("M4");
        os.Rejeitar("cliente", "preço alto");

        os.StatusOS.Should().Be(EnumStatusOS.Rejeitada);
        os.Historico.Last().Motivo.Should().Contain("preço alto");
    }

    [Fact]
    public void Rejeitar_ComMotivoVazio_LancaExcecao()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("mec");
        os.EnviarParaAprovacao("M4");

        Action act = () => os.Rejeitar("cliente", " ");
        act.Should().Throw<ArgumentException>().WithMessage("*motivo*");
    }

    [Fact]
    public void Finalizar_DeEmExecucao_TransitaParaFinalizada()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("mec");
        os.EnviarParaAprovacao("M4");
        os.Aprovar("cli");
        os.Finalizar("mec");

        os.StatusOS.Should().Be(EnumStatusOS.Finalizada);
    }

    [Fact]
    public void Entregar_DeFinalizada_TransitaParaEntregueEPreencheDataFechamento()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("mec");
        os.EnviarParaAprovacao("M4");
        os.Aprovar("cli");
        os.Finalizar("mec");
        os.Entregar("admin");

        os.StatusOS.Should().Be(EnumStatusOS.Entregue);
        os.DataFechamento.Should().NotBeNull();
        os.DataFechamento!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    // ----- TRANSIÇÕES INVÁLIDAS -----

    [Fact]
    public void Aprovar_DeRecebida_LancaExcecao()
    {
        var os = NovaOS();
        Action act = () => os.Aprovar("cli");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Recebida*AguardandoAprovacao*");
    }

    [Fact]
    public void IniciarDiagnostico_DeEntregue_LancaExcecao()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("m");
        os.EnviarParaAprovacao("M4");
        os.Aprovar("c");
        os.Finalizar("m");
        os.Entregar("a");

        Action act = () => os.IniciarDiagnostico("m");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IniciarDiagnostico_DeRejeitada_LancaExcecao()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("m");
        os.EnviarParaAprovacao("M4");
        os.Rejeitar("c", "nao quero");

        Action act = () => os.IniciarDiagnostico("m");
        act.Should().Throw<InvalidOperationException>();
    }

    // ----- OVERRIDE ADMINISTRATIVO -----

    [Fact]
    public void ForcarStatus_PermiteQualquerTransicao_RegistraOverride()
    {
        var os = NovaOS();
        os.ForcarStatus(EnumStatusOS.Entregue, "admin", "correção manual");

        os.StatusOS.Should().Be(EnumStatusOS.Entregue);
        os.Historico.Last().Motivo.Should().StartWith("Override administrativo");
        os.DataFechamento.Should().NotBeNull();
    }

    [Fact]
    public void ForcarStatus_ComMotivoVazio_LancaExcecao()
    {
        var os = NovaOS();
        Action act = () => os.ForcarStatus(EnumStatusOS.Entregue, "admin", " ");
        act.Should().Throw<ArgumentException>();
    }
}
