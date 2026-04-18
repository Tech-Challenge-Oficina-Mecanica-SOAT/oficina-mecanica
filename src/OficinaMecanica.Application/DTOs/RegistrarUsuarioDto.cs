using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Application.DTOs;

public record RegistrarUsuarioDto(string Email, string Senha, Perfil Perfil = Perfil.Admin);
