using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Interfaces;

public interface ITokenGenerator
{
    TokenDto GerarParaUsuario(Usuario usuario);
}
