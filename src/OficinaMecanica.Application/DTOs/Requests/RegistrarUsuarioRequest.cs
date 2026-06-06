using OficinaMecanica.Domain.Enums;
namespace OficinaMecanica.Application.DTOs.Requests;
public record RegistrarUsuarioRequest(string Email, string Senha, Perfil Perfil = Perfil.Cliente);
