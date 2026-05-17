using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculos;

public class ListarVeiculosUseCase : IListarVeiculosUseCase
{
    private readonly IVeiculoRepository _repository;
    private readonly VeiculoMapper _mapper;

    public ListarVeiculosUseCase(IVeiculoRepository repository, VeiculoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<VeiculoResponse>>> ExecutarAsync(Unit _)
    {
        var veiculos = await _repository.GetAllAsync();
        return Result<IEnumerable<VeiculoResponse>>.Success(veiculos.Select(_mapper.MapToResponse));
    }
}
