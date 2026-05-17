using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Cliente.AtivarCliente;

public class AtivarClienteUseCase : IAtivarClienteUseCase
{
    private readonly IClienteRepository _repository;

    public AtivarClienteUseCase(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        if (cliente is null)
            return Result<bool>.NotFound("Cliente não encontrado.");

        cliente.Ativar();
        await _repository.UpdateAsync(cliente);
        return Result<bool>.Success(true);
    }
}
