using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Cliente.DesativarCliente;

public class DesativarClienteUseCase : IDesativarClienteUseCase
{
    private readonly IClienteRepository _repository;

    public DesativarClienteUseCase(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        if (cliente is null)
            return Result<bool>.NotFound("Cliente não encontrado.");

        cliente.Desativar();
        await _repository.UpdateAsync(cliente);
        return Result<bool>.Success(true);
    }
}
