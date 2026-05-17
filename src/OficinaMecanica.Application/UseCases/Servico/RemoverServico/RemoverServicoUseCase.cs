using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Servico.RemoverServico;

public class RemoverServicoUseCase : IRemoverServicoUseCase
{
    private readonly IServicoRepository _repository;

    public RemoverServicoUseCase(IServicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        return Result<bool>.Success(true);
    }
}
