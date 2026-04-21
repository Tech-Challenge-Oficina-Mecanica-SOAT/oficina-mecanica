using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Interfaces;

public interface IJwtService
{
    TokenDto GerarToken(Usuario usuario);
}
