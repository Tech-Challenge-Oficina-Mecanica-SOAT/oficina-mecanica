using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Peca.ObterEstoque;

public class ObterEstoqueUseCase : IObterEstoqueUseCase
{
    private readonly IPecaInsumoRepository _repository;

    public ObterEstoqueUseCase(IPecaInsumoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<int>> ExecutarAsync(Guid id)
    {
        var estoque = await _repository.GetEstoqueAsync(id);
        return Result<int>.Success(estoque);
    }
}
