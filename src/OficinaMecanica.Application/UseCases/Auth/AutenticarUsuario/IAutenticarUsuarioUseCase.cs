using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Auth.AutenticarUsuario;

public interface IAutenticarUsuarioUseCase : IUseCase<LoginRequest, TokenResponse> { }
