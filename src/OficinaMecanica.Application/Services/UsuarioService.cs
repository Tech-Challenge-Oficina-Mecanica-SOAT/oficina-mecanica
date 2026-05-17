using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _hasher;

    public UsuarioService(IUsuarioRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        var usuario = await _repository.ObterPorEmailAsync(email.ToLower().Trim());
        if (usuario is null) return null;
        return _hasher.Verificar(senha, usuario.SenhaHash) ? usuario : null;
    }

    public async Task<Usuario> RegistrarAsync(RegistrarUsuarioDto dto)
    {
        var existente = await _repository.ObterPorEmailAsync(dto.Email.ToLower().Trim());
        if (existente is not null)
            throw new InvalidOperationException("Email já cadastrado.");

        var hash = _hasher.Hash(dto.Senha);
        var usuario = new Usuario(dto.Email, hash, dto.Perfil);
        await _repository.AdicionarAsync(usuario);
        return usuario;
    }
}
