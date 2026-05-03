using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OficinaMecanica.Application.Services;

public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiracaoMinutos;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey não configurada.");
        _issuer = configuration["Jwt:Issuer"] ?? "mecanica-api";
        _audience = configuration["Jwt:Audience"] ?? "mecanica-cliente";
        _expiracaoMinutos = int.TryParse(configuration["Jwt:ExpiracaoMinutos"], out var min) ? min : 5;
    }

    public TokenDto GerarToken(Usuario usuario)
    {
        var expiracao = DateTime.UtcNow.AddMinutes(_expiracaoMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: creds);

        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiracao);
    }
}
