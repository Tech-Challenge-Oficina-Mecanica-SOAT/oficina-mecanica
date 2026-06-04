using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class OrdemServicoMapper
{
    public OrdemServicoResponse MapToResponse(OrdemServico os) => new()
    {
        Id = os.Id,
        ClienteId = os.ClienteId,
        ClienteNome = os.Cliente?.Nome ?? string.Empty,
        VeiculoId = os.VeiculoId,
        VeiculoDescricao = os.Veiculo != null
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} ({os.Veiculo.Placa})"
            : string.Empty,
        Status = os.StatusOS.ToString(),
        Observacoes = os.Observacoes,
        Total = os.Total,
        DataAbertura = os.DataAbertura,
        DataFechamento = os.DataFechamento,
        Itens = os.Itens.Select(MapToItemResponse).ToList()
    };

    public OrdemServicoResumoResponse MapToResumoResponse(OrdemServico os) => new()
    {
        Id = os.Id,
        ClienteNome = os.Cliente?.Nome ?? string.Empty,
        VeiculoDescricao = os.Veiculo != null
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} ({os.Veiculo.Placa})"
            : string.Empty,
        Status = os.StatusOS.ToString(),
        Total = os.Total,
        DataAbertura = os.DataAbertura,
        DataFechamento = os.DataFechamento
    };

    public OrdemServicoItemResponse MapToItemResponse(OrdemServicoItem item) => new()
    {
        Id = item.Id,
        Tipo = item.Tipo.ToString().ToLower(),
        ReferenciaId = item.ReferenciaId,
        Descricao = item.Descricao,
        Quantidade = item.Quantidade,
        PrecoUnitario = item.PrecoUnitario,
        Subtotal = item.Subtotal
    };
}
