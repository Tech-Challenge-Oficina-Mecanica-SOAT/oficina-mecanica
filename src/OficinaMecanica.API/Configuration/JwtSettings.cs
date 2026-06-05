using Microsoft.Extensions.Configuration;
using OficinaMecanica.Application.Configuration;

namespace OficinaMecanica.API.Configuration;

public class JwtSettings : IJwtSettings
{
    public string SecretKey { get; }
    public string Issuer { get; }
    public string Audience { get; }
    public int ExpiracaoMinutos { get; }

    public JwtSettings(IConfiguration configuration)
    {
        SecretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey não configurada.");
        Issuer = configuration["Jwt:Issuer"] ?? "mecanica-api";
        Audience = configuration["Jwt:Audience"] ?? "mecanica-cliente";
        var expiracaoStr = configuration["Jwt:ExpiracaoMinutos"];
        if (string.IsNullOrWhiteSpace(expiracaoStr))
        {
            ExpiracaoMinutos = 5;
        }
        else if (int.TryParse(expiracaoStr, out var min) && min > 0)
        {
            ExpiracaoMinutos = min;
        }
        else
        {
            throw new InvalidOperationException(
                $"Jwt:ExpiracaoMinutos invalido: '{expiracaoStr}'. Deve ser inteiro positivo.");
        }
    }
}
