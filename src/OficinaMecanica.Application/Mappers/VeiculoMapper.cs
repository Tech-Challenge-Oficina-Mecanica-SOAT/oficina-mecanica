using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Mappers;

public class VeiculoMapper
{
    public VeiculoResponse MapToResponse(Veiculo veiculo) => new()
    {
        Id = veiculo.Id,
        ClienteId = veiculo.ClienteId,
        ClienteNome = veiculo.Cliente?.Nome ?? string.Empty,
        Placa = veiculo.Placa,
        Marca = veiculo.Marca,
        Modelo = veiculo.Modelo,
        Ano = veiculo.Ano,
        CriadoEm = veiculo.CriadoEm
    };
}
