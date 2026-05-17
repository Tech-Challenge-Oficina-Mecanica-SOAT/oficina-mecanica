using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Veiculo.RemoverVeiculo;

public class RemoverVeiculoUseCase : IRemoverVeiculoUseCase
{
    private readonly IVeiculoRepository _repository;

    public RemoverVeiculoUseCase(IVeiculoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        return Result<bool>.Success(true);
    }
}
