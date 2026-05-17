using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Interfaces;

public interface ITokenGenerator
{
    TokenResponse GerarParaUsuario(Usuario usuario);
}
