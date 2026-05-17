namespace OficinaMecanica.Application.Interfaces;

public interface IAppLogger<T>
{
    void Info(string message, params object[] args);
    void Warning(string message, params object[] args);
    void Warning(string message, Exception ex, params object[] args);
    void Error(string message, params object[] args);
    void Error(string message, Exception ex, params object[] args);
}
