using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Auth.AutenticarUsuario;

public class AutenticarUsuarioUseCase : IAutenticarUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokenGenerator;

    public AutenticarUsuarioUseCase(
        IUsuarioRepository repository,
        IPasswordHasher hasher,
        ITokenGenerator tokenGenerator)
    {
        _repository = repository;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<TokenResponse>> ExecutarAsync(LoginRequest request)
    {
        var usuario = await _repository.ObterPorEmailAsync(request.Email.ToLower().Trim());
        if (usuario is null)
            return Result<TokenResponse>.Unauthorized("Credenciais inválidas.");

        if (!_hasher.Verificar(request.Senha, usuario.SenhaHash))
            return Result<TokenResponse>.Unauthorized("Credenciais inválidas.");

        return Result<TokenResponse>.Success(_tokenGenerator.GerarParaUsuario(usuario));
    }
}
