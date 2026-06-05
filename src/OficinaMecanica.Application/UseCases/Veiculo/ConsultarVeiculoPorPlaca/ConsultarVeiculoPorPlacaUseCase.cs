using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

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
        Placa placaVO;
        try
        {
            placaVO = new Placa(placa);
        }
        catch (ArgumentException ex)
        {
            return Result<VeiculoResponse>.Validation(ex.Message);
        }

        var veiculo = await _repository.GetByPlacaAsync(placaVO.Valor);
        return veiculo is null
            ? Result<VeiculoResponse>.NotFound("Veículo não encontrado.")
            : Result<VeiculoResponse>.Success(_mapper.MapToResponse(veiculo));
    }
}
