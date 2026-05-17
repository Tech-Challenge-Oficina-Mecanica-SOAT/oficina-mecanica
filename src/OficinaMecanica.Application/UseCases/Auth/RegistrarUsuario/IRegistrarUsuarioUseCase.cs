using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;

namespace OficinaMecanica.Application.UseCases.Auth.RegistrarUsuario;

public interface IRegistrarUsuarioUseCase : IUseCase<RegistrarUsuarioRequest, Guid> { }
