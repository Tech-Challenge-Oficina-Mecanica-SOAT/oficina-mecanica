using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> AutenticarAsync(string email, string senha);
    Task<Usuario> RegistrarAsync(RegistrarUsuarioRequest dto);
}
