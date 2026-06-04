using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Veiculo.ConsultarVeiculo;

public class ConsultarVeiculoUseCase : IConsultarVeiculoUseCase
{
    private readonly IVeiculoRepository _repository;
    private readonly VeiculoMapper _mapper;

    public ConsultarVeiculoUseCase(IVeiculoRepository repository, VeiculoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<VeiculoResponse>> ExecutarAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        return veiculo is null
            ? Result<VeiculoResponse>.NotFound("Veículo não encontrado.")
            : Result<VeiculoResponse>.Success(_mapper.MapToResponse(veiculo));
    }
}
