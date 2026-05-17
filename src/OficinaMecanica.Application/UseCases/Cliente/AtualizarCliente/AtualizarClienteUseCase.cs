using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Application.UseCases.Cliente.AtualizarCliente;

public class AtualizarClienteUseCase : IAtualizarClienteUseCase
{
    private readonly IClienteRepository _repository;
    private readonly ClienteMapper _mapper;

    public AtualizarClienteUseCase(IClienteRepository repository, ClienteMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ClienteResponse>> ExecutarAsync(AtualizarClienteUseCaseRequest request)
    {
        var cliente = await _repository.GetByIdAsync(request.Id);
        if (cliente is null)
            return Result<ClienteResponse>.NotFound("Cliente não encontrado.");

        Email email;
        try
        {
            email = new Email(request.Email);
        }
        catch (ArgumentException ex)
        {
            return Result<ClienteResponse>.Validation(ex.Message);
        }

        try
        {
            cliente.Atualizar(request.Nome, request.Telefone, email);
        }
        catch (ArgumentException ex)
        {
            return Result<ClienteResponse>.Validation(ex.Message);
        }

        await _repository.UpdateAsync(cliente);
        return Result<ClienteResponse>.Success(_mapper.MapToResponse(cliente));
    }
}
