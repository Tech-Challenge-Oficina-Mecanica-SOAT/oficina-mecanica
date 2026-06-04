using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class ServicoMapper
{
    public ServicoResponse MapToResponse(Servico servico) => new()
    {
        Id = servico.Id,
        Nome = servico.Nome,
        Descricao = servico.Descricao,
        Valor = servico.Valor,
        Ativo = servico.Ativo,
        CriadoEm = servico.CriadoEm
    };
}
