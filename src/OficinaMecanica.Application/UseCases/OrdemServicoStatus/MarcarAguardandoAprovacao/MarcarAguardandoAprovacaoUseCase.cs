using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;

public class MarcarAguardandoAprovacaoUseCase : IMarcarAguardandoAprovacaoUseCase
{
    private readonly IOrdemServicoRepository _repository;

    public MarcarAguardandoAprovacaoUseCase(IOrdemServicoRepository repository) => _repository = repository;

    public async Task<Result<bool>> ExecutarAsync(MarcarAguardandoAprovacaoRequest request)
    {
        var os = await _repository.ObterPorIdAsync(request.OsId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        try { os.EnviarParaAprovacao(request.AlteradoPor); }
        catch (InvalidOperationException ex) { return Result<bool>.Validation(ex.Message); }

        await _repository.UpdateAsync(os);
        return Result<bool>.Success(true);
    }
}
