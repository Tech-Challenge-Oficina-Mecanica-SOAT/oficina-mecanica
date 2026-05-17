using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ObterTempoMedioExecucao;

public class ObterTempoMedioExecucaoUseCase : IObterTempoMedioExecucaoUseCase
{
    private readonly IOrdemServicoRepository _repository;

    public ObterTempoMedioExecucaoUseCase(IOrdemServicoRepository repository) => _repository = repository;

    public async Task<Result<double>> ExecutarAsync(Unit _)
    {
        var horas = await _repository.GetTempoMedioExecucaoHorasAsync();
        return Result<double>.Success(horas);
    }
}
