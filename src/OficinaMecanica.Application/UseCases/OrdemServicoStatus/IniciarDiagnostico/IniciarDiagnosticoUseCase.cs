using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.IniciarDiagnostico;

public class IniciarDiagnosticoUseCase : IIniciarDiagnosticoUseCase
{
    private readonly IOrdemServicoRepository _repository;

    public IniciarDiagnosticoUseCase(IOrdemServicoRepository repository) => _repository = repository;

    public async Task<Result<bool>> ExecutarAsync(IniciarDiagnosticoRequest request)
    {
        var os = await _repository.ObterPorIdAsync(request.OsId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        try { os.IniciarDiagnostico(request.AlteradoPor); }
        catch (InvalidOperationException ex) { return Result<bool>.Validation(ex.Message); }

        await _repository.UpdateAsync(os);
        return Result<bool>.Success(true);
    }
}
