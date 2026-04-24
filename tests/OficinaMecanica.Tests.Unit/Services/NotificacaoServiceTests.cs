using Microsoft.Extensions.Logging;
using Moq;
using OficinaMecanica.Application.Services;

namespace OficinaMecanica.Tests.Unit.Services;

public class NotificacaoServiceTests
{
    private readonly Mock<ILogger<NotificacaoService>> _loggerMock = new();
    private readonly NotificacaoService _sut;

    public NotificacaoServiceTests()
    {
        _sut = new NotificacaoService(_loggerMock.Object);
    }

    [Fact]
    public async Task EnviarOrcamentoAsync_CompletaSemExcecao()
    {
        await _sut.EnviarOrcamentoAsync(Guid.NewGuid(), "cliente@oficina.com", 1500.00m);
    }

    [Fact]
    public async Task EnviarOrcamentoAsync_RetornaTaskCompleted()
    {
        var task = _sut.EnviarOrcamentoAsync(Guid.NewGuid(), "cliente@oficina.com", 500.00m);

        await task;

        Assert.True(task.IsCompletedSuccessfully);
    }
}
