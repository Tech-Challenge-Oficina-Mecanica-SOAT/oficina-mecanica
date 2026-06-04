using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Application.Configuration;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OficinaMecanica.Infrastructure.Auth;

public class JwtTokenGenerator : ITokenGenerator
{
    private readonly IJwtSettings _settings;

    public JwtTokenGenerator(IJwtSettings settings) => _settings = settings;

    public TokenResponse GerarParaUsuario(Usuario usuario)
    {
        var expiracao = DateTime.UtcNow.AddMinutes(_settings.ExpiracaoMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: creds);

        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiracao);
    }
}
