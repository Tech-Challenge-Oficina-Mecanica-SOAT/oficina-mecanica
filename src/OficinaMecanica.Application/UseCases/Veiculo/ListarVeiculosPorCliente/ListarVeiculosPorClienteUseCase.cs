using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculosPorCliente;

public class ListarVeiculosPorClienteUseCase : IListarVeiculosPorClienteUseCase
{
    private readonly IVeiculoRepository _repository;
    private readonly VeiculoMapper _mapper;

    public ListarVeiculosPorClienteUseCase(IVeiculoRepository repository, VeiculoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<VeiculoResponse>>> ExecutarAsync(Guid clienteId)
    {
        var veiculos = await _repository.GetByClienteIdAsync(clienteId);
        return Result<IEnumerable<VeiculoResponse>>.Success(veiculos.Select(_mapper.MapToResponse));
    }
}
