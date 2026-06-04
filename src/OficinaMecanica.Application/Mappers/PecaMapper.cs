using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class PecaMapper
{
    public PecaResponse MapToResponse(PecaInsumo peca) => new()
    {
        Id = peca.Id,
        Nome = peca.Nome,
        Codigo = peca.Codigo,
        Descricao = peca.Descricao,
        PrecoUnitario = peca.Preco,
        Estoque = peca.Quantidade,
        CriadoEm = peca.CriadoEm,
        Ativo = peca.Ativo
    };
}
