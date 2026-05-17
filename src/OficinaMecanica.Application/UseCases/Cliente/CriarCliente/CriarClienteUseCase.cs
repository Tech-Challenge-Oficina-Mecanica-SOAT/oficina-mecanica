using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Cliente.CriarCliente;

public class CriarClienteUseCase : ICriarClienteUseCase
{
    private readonly IClienteRepository _repository;
    private readonly ClienteMapper _mapper;

    public CriarClienteUseCase(IClienteRepository repository, ClienteMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ClienteResponse>> ExecutarAsync(CriarClienteRequest request)
    {
        if (await _repository.ExistsByDocumentoAsync(request.Documento))
            return Result<ClienteResponse>.Conflict("Cliente com este documento já cadastrado.");

        try
        {
            var cliente = new Domain.Entities.Cliente(request.Nome, request.Documento, request.Telefone, request.Email);
            var criado = await _repository.AddAsync(cliente);
            return Result<ClienteResponse>.Success(_mapper.MapToResponse(criado));
        }
        catch (ArgumentException ex)
        {
            return Result<ClienteResponse>.Validation(ex.Message);
        }
    }
}
