using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.NotificarConclusao;

public class NotificarConclusaoUseCase : INotificarConclusaoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly IDomainEventDispatcher _dispatcher;

    public NotificarConclusaoUseCase(IOrdemServicoRepository repository, IDomainEventDispatcher dispatcher)
    {
        _repository = repository;
        _dispatcher = dispatcher;
    }

    public async Task<Result<bool>> ExecutarAsync(NotificarConclusaoRequest request)
    {
        var os = await _repository.ObterPorIdAsync(request.OsId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        try { os.Finalizar(request.AlteradoPor); }
        catch (InvalidOperationException ex) { return Result<bool>.Validation(ex.Message); }

        await _repository.UpdateAsync(os);
        await _dispatcher.DispatchAsync(os.DomainEvents);
        return Result<bool>.Success(true);
    }
}
