using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Cliente.ListarClientes;

public class ListarClientesUseCase : IListarClientesUseCase
{
    private readonly IClienteRepository _repository;
    private readonly ClienteMapper _mapper;

    public ListarClientesUseCase(IClienteRepository repository, ClienteMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ClienteResponse>>> ExecutarAsync(Unit _)
    {
        var clientes = await _repository.GetAllAsync();
        return Result<IEnumerable<ClienteResponse>>.Success(clientes.Select(_mapper.MapToResponse));
    }
}
