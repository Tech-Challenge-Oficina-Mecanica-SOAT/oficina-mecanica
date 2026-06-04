using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.NotificarConclusao;

public class NotificarConclusaoUseCase : INotificarConclusaoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly INotificacaoService _notificacao;
    private readonly IAppLogger<NotificarConclusaoUseCase> _logger;

    public NotificarConclusaoUseCase(
        IOrdemServicoRepository repository,
        INotificacaoService notificacao,
        IAppLogger<NotificarConclusaoUseCase> logger)
    {
        _repository = repository;
        _notificacao = notificacao;
        _logger = logger;
    }

    public async Task<Result<bool>> ExecutarAsync(NotificarConclusaoRequest request)
    {
        var os = await _repository.ObterPorIdAsync(request.OsId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        try { os.Finalizar(request.AlteradoPor); }
        catch (InvalidOperationException ex) { return Result<bool>.Validation(ex.Message); }

        try
        {
            await _notificacao.EnviarConclusaoAsync(request.OsId, os.Cliente?.Email ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.Warning("Falha ao enviar notificação de conclusão para OS {OsId}.", ex, request.OsId);
        }

        await _repository.UpdateAsync(os);
        return Result<bool>.Success(true);
    }
}
