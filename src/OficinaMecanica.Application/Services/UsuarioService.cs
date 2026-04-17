using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly byte[] _passwordKey;

    public UsuarioService(IUsuarioRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        var key = configuration["Seguranca:PasswordKey"]
            ?? throw new InvalidOperationException("Seguranca:PasswordKey não configurada.");
        _passwordKey = Encoding.UTF8.GetBytes(key);
    }

    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        var usuario = await _repository.ObterPorEmailAsync(email.ToLower().Trim());
        if (usuario is null)
            return null;

        return VerificarSenha(senha, usuario.SenhaHash) ? usuario : null;
    }

    public async Task<Usuario> RegistrarAsync(RegistrarUsuarioDto dto)
    {
        var existente = await _repository.ObterPorEmailAsync(dto.Email.ToLower().Trim());
        if (existente is not null)
            throw new InvalidOperationException("Email já cadastrado.");

        var hash = HashSenha(dto.Senha);
        var usuario = new Usuario(dto.Email, hash, dto.Perfil);
        await _repository.AdicionarAsync(usuario);
        return usuario;
    }

    private string HashSenha(string senha)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(32);
        byte[] hash = ComputeHmac(salt, senha);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private bool VerificarSenha(string senha, string senhaHash)
    {
        var parts = senhaHash.Split(':');
        if (parts.Length != 2) return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);
        byte[] actualHash = ComputeHmac(salt, senha);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private byte[] ComputeHmac(byte[] salt, string senha)
    {
        using var hmac = new HMACSHA256(_passwordKey);
        var data = salt.Concat(Encoding.UTF8.GetBytes(senha)).ToArray();
        return hmac.ComputeHash(data);
    }
}
