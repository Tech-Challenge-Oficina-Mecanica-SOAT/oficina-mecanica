using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> AutenticarAsync(string email, string senha);
    Task<Usuario> RegistrarAsync(RegistrarUsuarioDto dto);
}
