using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class ClienteMapper
{
    public ClienteResponse MapToResponse(Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nome = cliente.Nome,
        Documento = cliente.Documento,
        Telefone = cliente.Telefone,
        Email = cliente.Email,
        Ativo = cliente.Ativo,
        CriadoEm = cliente.CriadoEm
    };
}
