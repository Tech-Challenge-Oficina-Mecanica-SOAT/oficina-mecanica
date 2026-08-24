using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.ForcarStatusOS;

public class ForcarStatusOSUseCase : IForcarStatusOSUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly IOrdemServicoMetrics _metrics;

    public ForcarStatusOSUseCase(IOrdemServicoRepository repository, IOrdemServicoMetrics metrics)
    {
        _repository = repository;
        _metrics = metrics;
    }

    public async Task<Result<bool>> ExecutarAsync(ForcarStatusOSRequest request)
    {
        var os = await _repository.ObterPorIdAsync(request.OsId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        var statusAnterior = os.StatusOS;
        try { os.ForcarStatus(request.NovoStatus, request.AlteradoPor, request.Motivo); }
        catch (InvalidOperationException ex) { return Result<bool>.Validation(ex.Message); }

        await _repository.UpdateAsync(os);
        _metrics.AtualizarStatus(statusAnterior, os.StatusOS);
        return Result<bool>.Success(true);
    }
}
