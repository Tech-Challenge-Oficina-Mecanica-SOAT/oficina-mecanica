using Microsoft.Extensions.Configuration;
using OficinaMecanica.Application.Configuration;

namespace OficinaMecanica.API.Configuration;

public class PasswordSettings : IPasswordSettings
{
    public string PasswordKey { get; }

    public PasswordSettings(IConfiguration configuration)
    {
        PasswordKey = configuration["Seguranca:PasswordKey"]
            ?? throw new InvalidOperationException("Seguranca:PasswordKey não configurada.");
    }
}
