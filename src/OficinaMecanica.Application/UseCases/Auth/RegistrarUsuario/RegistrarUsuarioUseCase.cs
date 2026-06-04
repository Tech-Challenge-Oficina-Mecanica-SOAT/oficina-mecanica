using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Auth.RegistrarUsuario;

public class RegistrarUsuarioUseCase : IRegistrarUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _hasher;

    public RegistrarUsuarioUseCase(IUsuarioRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public async Task<Result<Guid>> ExecutarAsync(RegistrarUsuarioRequest request)
    {
        var existente = await _repository.ObterPorEmailAsync(request.Email.ToLower().Trim());
        if (existente is not null)
            return Result<Guid>.Conflict("Email já cadastrado.");

        var hash = _hasher.Hash(request.Senha);
        var usuario = new Usuario(request.Email, hash, request.Perfil);
        await _repository.AdicionarAsync(usuario);
        return Result<Guid>.Success(usuario.Id);
    }
}
