using Microsoft.Extensions.Logging;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.Infrastructure.Logging;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public AppLogger(ILogger<T> logger) => _logger = logger;

    public void Info(string message, params object[] args) =>
        _logger.LogInformation(message, args);

    public void Warning(string message, params object[] args) =>
        _logger.LogWarning(message, args);

    public void Warning(string message, Exception ex, params object[] args) =>
        _logger.LogWarning(ex, message, args);

    public void Error(string message, params object[] args) =>
        _logger.LogError(message, args);

    public void Error(string message, Exception ex, params object[] args) =>
        _logger.LogError(ex, message, args);
}
