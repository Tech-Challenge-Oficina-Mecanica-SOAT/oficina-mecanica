using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Cliente.ConsultarClientePorDocumento;

public class ConsultarClientePorDocumentoUseCase : IConsultarClientePorDocumentoUseCase
{
    private readonly IClienteRepository _repository;
    private readonly ClienteMapper _mapper;

    public ConsultarClientePorDocumentoUseCase(IClienteRepository repository, ClienteMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ClienteResponse>> ExecutarAsync(string documento)
    {
        var cliente = await _repository.GetByDocumentoAsync(documento);
        return cliente is null
            ? Result<ClienteResponse>.NotFound("Cliente não encontrado.")
            : Result<ClienteResponse>.Success(_mapper.MapToResponse(cliente));
    }
}
