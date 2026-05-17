using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Tests.Unit.Services;

public class OrdemServicoStatusServiceTests
{
    private readonly Mock<IOrdemServicoRepository> _osRepo = new();
    private readonly Mock<INotificacaoService> _notificacao = new();
    private readonly Mock<IAppLogger<OrdemServicoStatusService>> _logger = new();
    private readonly OrdemServicoStatusService _service;

    public OrdemServicoStatusServiceTests()
    {
        _service = new OrdemServicoStatusService(
            _osRepo.Object,
            _notificacao.Object,
            _logger.Object);
    }

    private OrdemServico OSRecebida() => new(Guid.NewGuid(), Guid.NewGuid(), "obs");

    [Fact]
    public async Task IniciarDiagnosticoAsync_QuandoOSExiste_TransitaEPersiste()
    {
        var os = OSRecebida();
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        await _service.IniciarDiagnosticoAsync(os.Id, "mec");

        os.StatusOS.Should().Be(EnumStatusOS.EmDiagnostico);
        _osRepo.Verify(r => r.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task IniciarDiagnosticoAsync_QuandoOSNaoExiste_LancaKeyNotFound()
    {
        _osRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        Func<Task> act = () => _service.IniciarDiagnosticoAsync(Guid.NewGuid(), "mec");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task MarcarAguardandoAprovacaoAsync_DeEmDiagnostico_TransitaEPersiste()
    {
        var os = OSRecebida();
        os.IniciarDiagnostico("mec");
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        await _service.MarcarAguardandoAprovacaoAsync(os.Id, "M4");

        os.StatusOS.Should().Be(EnumStatusOS.AguardandoAprovacao);
        _osRepo.Verify(r => r.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task AprovarAsync_QuandoStatusInvalido_PropagaInvalidOperation()
    {
        var os = OSRecebida();
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        Func<Task> act = () => _service.AprovarAsync(os.Id, "cli");

        await act.Should().ThrowAsync<InvalidOperationException>();
        _osRepo.Verify(r => r.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task RejeitarAsync_FluxoCompleto_AtualizaStatusEMotivo()
    {
        var os = OSRecebida();
        os.IniciarDiagnostico("m");
        os.EnviarParaAprovacao("M4");
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        await _service.RejeitarAsync(os.Id, "cli", "preço alto");

        os.StatusOS.Should().Be(EnumStatusOS.Rejeitada);
        os.Historico.Last().Motivo.Should().Contain("preço alto");
    }

    [Fact]
    public async Task NotificarConclusaoAsync_FluxoFeliz_EnviaEmailEFinaliza()
    {
        var os = await CriarOSEmExecucaoAsync();
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        await _service.NotificarConclusaoAsync(os.Id, "mec");

        os.StatusOS.Should().Be(EnumStatusOS.Finalizada);
        _notificacao.Verify(n => n.EnviarConclusaoAsync(os.Id, os.Cliente.Email), Times.Once);
        _osRepo.Verify(r => r.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task NotificarConclusaoAsync_QuandoEmailFalha_AindaFinalizaEPersiste()
    {
        var os = await CriarOSEmExecucaoAsync();
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);
        _notificacao
            .Setup(n => n.EnviarConclusaoAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("SMTP down"));

        await _service.NotificarConclusaoAsync(os.Id, "mec");

        os.StatusOS.Should().Be(EnumStatusOS.Finalizada);
        _osRepo.Verify(r => r.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task NotificarConclusaoAsync_QuandoOSNaoExiste_LancaKeyNotFound()
    {
        _osRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        Func<Task> act = () => _service.NotificarConclusaoAsync(Guid.NewGuid(), "mec");

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _notificacao.Verify(n => n.EnviarConclusaoAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task NotificarConclusaoAsync_QuandoStatusInvalido_NaoEnviaEmail()
    {
        var os = OSRecebida(); // em Recebida, não pode Finalizar
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        Func<Task> act = () => _service.NotificarConclusaoAsync(os.Id, "mec");

        await act.Should().ThrowAsync<InvalidOperationException>();
        _notificacao.Verify(n => n.EnviarConclusaoAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _osRepo.Verify(r => r.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task EntregarAsync_FluxoCompleto_PreencheDataFechamento()
    {
        var os = await CriarOSEmExecucaoAsync();
        os.Finalizar("mec");
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        await _service.EntregarAsync(os.Id, "admin");

        os.StatusOS.Should().Be(EnumStatusOS.Entregue);
        os.DataFechamento.Should().NotBeNull();
    }

    [Fact]
    public async Task ForcarStatusAsync_PermiteTransicaoArbitraria()
    {
        var os = OSRecebida();
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id)).ReturnsAsync(os);

        await _service.ForcarStatusAsync(os.Id, EnumStatusOS.Finalizada, "admin", "correção");

        os.StatusOS.Should().Be(EnumStatusOS.Finalizada);
    }

    [Fact]
    public async Task ObterHistoricoAsync_RetornaListaMapeada()
    {
        var os = OSRecebida();
        os.IniciarDiagnostico("mec");
        _osRepo.Setup(r => r.ObterPorIdComHistoricoAsync(os.Id)).ReturnsAsync(os);

        var result = (await _service.ObterHistoricoAsync(os.Id)).ToList();

        result.Should().HaveCount(2);
        result[0].StatusAnterior.Should().BeNull();
        result[0].StatusNovo.Should().Be("Recebida");
        result[1].StatusAnterior.Should().Be("Recebida");
        result[1].StatusNovo.Should().Be("EmDiagnostico");
    }

    private static Task<OrdemServico> CriarOSEmExecucaoAsync()
    {
        var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
        os.IniciarDiagnostico("setup");
        os.EnviarParaAprovacao("setup");
        os.Aprovar("setup");

        var cliente = new Cliente("Tester", "12345678909", "(11) 99999-0000", "tester@oficina.com");
        typeof(OrdemServico).GetProperty(nameof(OrdemServico.Cliente))!
            .SetValue(os, cliente, null);
        return Task.FromResult(os);
    }
}
