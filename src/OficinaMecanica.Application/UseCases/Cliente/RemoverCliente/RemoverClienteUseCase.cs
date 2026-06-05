using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Cliente.RemoverCliente;

public class RemoverClienteUseCase : IRemoverClienteUseCase
{
    private readonly IClienteRepository _repository;

    public RemoverClienteUseCase(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        if (cliente is null)
            return Result<bool>.NotFound($"Cliente com ID {id} não encontrado.");

        await _repository.DeleteAsync(id);
        return Result<bool>.Success(true);
    }
}
