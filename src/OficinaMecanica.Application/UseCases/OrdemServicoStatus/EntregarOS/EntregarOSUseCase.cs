using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.EntregarOS;

public class EntregarOSUseCase : IEntregarOSUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly IOrdemServicoMetrics _metrics;

    public EntregarOSUseCase(IOrdemServicoRepository repository, IOrdemServicoMetrics metrics)
    {
        _repository = repository;
        _metrics = metrics;
    }

    public async Task<Result<bool>> ExecutarAsync(EntregarOSUseCaseRequest request)
    {
        var os = await _repository.ObterPorIdAsync(request.OsId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        var statusAnterior = os.StatusOS;
        try { os.Entregar(request.AlteradoPor); }
        catch (InvalidOperationException ex) { return Result<bool>.Validation(ex.Message); }

        await _repository.UpdateAsync(os);
        _metrics.AtualizarStatus(statusAnterior, os.StatusOS);
        if (os.DataFechamento.HasValue)
            _metrics.RegistrarTempoExecucao(os.DataFechamento.Value - os.DataAbertura);

        return Result<bool>.Success(true);
    }
}
