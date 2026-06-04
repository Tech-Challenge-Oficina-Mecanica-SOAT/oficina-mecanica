using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Veiculo.ConsultarVeiculoPorPlaca;

public class ConsultarVeiculoPorPlacaUseCase : IConsultarVeiculoPorPlacaUseCase
{
    private readonly IVeiculoRepository _repository;
    private readonly VeiculoMapper _mapper;

    public ConsultarVeiculoPorPlacaUseCase(IVeiculoRepository repository, VeiculoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<VeiculoResponse>> ExecutarAsync(string placa)
    {
        var veiculo = await _repository.GetByPlacaAsync(placa);
        return veiculo is null
            ? Result<VeiculoResponse>.NotFound("Veículo não encontrado.")
            : Result<VeiculoResponse>.Success(_mapper.MapToResponse(veiculo));
    }
}
