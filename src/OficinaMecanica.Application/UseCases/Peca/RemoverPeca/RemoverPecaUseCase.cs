using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Peca.RemoverPeca;

public class RemoverPecaUseCase : IRemoverPecaUseCase
{
    private readonly IPecaInsumoRepository _repository;

    public RemoverPecaUseCase(IPecaInsumoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        var peca = await _repository.GetByIdAsync(id);
        if (peca is null)
            return Result<bool>.NotFound($"Peça com ID {id} não encontrada");

        await _repository.DeleteAsync(id);
        return Result<bool>.Success(true);
    }
}
