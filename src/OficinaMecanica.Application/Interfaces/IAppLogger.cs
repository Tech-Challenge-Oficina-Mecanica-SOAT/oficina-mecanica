namespace OficinaMecanica.Application.Interfaces;

public interface IAppLogger<T>
{
    void Info(string message, params object[] args);
    void Warning(string message, Exception? ex = null, params object[] args);
    void Error(string message, Exception? ex = null, params object[] args);
}
