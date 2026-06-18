using FluentAssertions;
using Moq;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Infrastructure.Notifications;
using Xunit;

namespace OficinaMecanica.Tests.Unit.Infrastructure.Notifications;

public class EmailNotificacaoServiceFakeTests
{
    private readonly EmailNotificacaoServiceFake _service;
    private readonly Mock<IAppLogger<EmailNotificacaoServiceFake>> _loggerMock;

    public EmailNotificacaoServiceFakeTests()
    {
        _loggerMock = new Mock<IAppLogger<EmailNotificacaoServiceFake>>();
        _service = new EmailNotificacaoServiceFake(_loggerMock.Object);
    }

    [Fact]
    public async Task EnviarOrcamentoAsync_DeveLogarMensagem()
    {
        await _service.EnviarOrcamentoAsync(Guid.NewGuid(), "teste@email.com", 100m);
        _loggerMock.Verify(x => x.Info(It.Is<string>(s => s.Contains("FAKE") && s.Contains("EnviarOrcamentoAsync"))), Times.Once);
    }

    [Fact]
    public async Task EnviarAprovacaoAsync_DeveLogarMensagem()
    {
        await _service.EnviarAprovacaoAsync(Guid.NewGuid(), "teste@email.com");
        _loggerMock.Verify(x => x.Info(It.Is<string>(s => s.Contains("FAKE") && s.Contains("EnviarAprovacaoAsync"))), Times.Once);
    }

    [Fact]
    public async Task EnviarRejeicaoAsync_DeveLogarMensagem()
    {
        await _service.EnviarRejeicaoAsync(Guid.NewGuid(), "teste@email.com", "motivo");
        _loggerMock.Verify(x => x.Info(It.Is<string>(s => s.Contains("FAKE") && s.Contains("EnviarRejeicaoAsync"))), Times.Once);
    }

    [Fact]
    public async Task EnviarConclusaoAsync_DeveLogarMensagem()
    {
        await _service.EnviarConclusaoAsync(Guid.NewGuid(), "teste@email.com", "motivo");
        _loggerMock.Verify(x => x.Info(It.Is<string>(s => s.Contains("FAKE") && s.Contains("EnviarConclusaoAsync"))), Times.Once);
    }

    [Fact]
    public async Task EnviarEntregaAsync_DeveLogarMensagem()
    {
        await _service.EnviarEntregaAsync(Guid.NewGuid(), "teste@email.com");
        _loggerMock.Verify(x => x.Info(It.Is<string>(s => s.Contains("FAKE") && s.Contains("EnviarEntregaAsync"))), Times.Once);
    }

    [Fact]
    public async Task EnviarCriacaoAsync_DeveLogarMensagem()
    {
        await _service.EnviarCriacaoAsync(Guid.NewGuid(), "teste@email.com");
        _loggerMock.Verify(x => x.Info(It.Is<string>(s => s.Contains("FAKE") && s.Contains("EnviarCriacaoAsync"))), Times.Once);
    }

    [Fact]
    public async Task EnviarDiagnosticoAsync_DeveLogarMensagem()
    {
        await _service.EnviarDiagnosticoAsync(Guid.NewGuid(), "teste@email.com");
        _loggerMock.Verify(x => x.Info(It.Is<string>(s => s.Contains("FAKE") && s.Contains("EnviarDiagnosticoAsync"))), Times.Once);
    }
}