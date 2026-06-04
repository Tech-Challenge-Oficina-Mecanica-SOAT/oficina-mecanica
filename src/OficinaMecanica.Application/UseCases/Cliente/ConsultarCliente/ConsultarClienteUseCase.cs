using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Cliente.ConsultarCliente;

public class ConsultarClienteUseCase : IConsultarClienteUseCase
{
    private readonly IClienteRepository _repository;
    private readonly ClienteMapper _mapper;

    public ConsultarClienteUseCase(IClienteRepository repository, ClienteMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ClienteResponse>> ExecutarAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        return cliente is null
            ? Result<ClienteResponse>.NotFound("Cliente não encontrado.")
            : Result<ClienteResponse>.Success(_mapper.MapToResponse(cliente));
    }
}
