using Microsoft.Extensions.Logging;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services;

public class OrdemServicoStatusService : IOrdemServicoStatusService
{
    private readonly IOrdemServicoRepository _osRepository;
    private readonly IHistoricoStatusOSRepository _historicoRepository;
    private readonly INotificacaoService _notificacaoService;
    private readonly ILogger<OrdemServicoStatusService> _logger;

    public OrdemServicoStatusService(
        IOrdemServicoRepository osRepository,
        IHistoricoStatusOSRepository historicoRepository,
        INotificacaoService notificacaoService,
        ILogger<OrdemServicoStatusService> logger)
    {
        _osRepository = osRepository;
        _historicoRepository = historicoRepository;
        _notificacaoService = notificacaoService;
        _logger = logger;
    }

    public Task IniciarDiagnosticoAsync(Guid osId, string alteradoPor) =>
        AplicarTransicaoAsync(osId, os => os.IniciarDiagnostico(alteradoPor));

    public Task MarcarAguardandoAprovacaoAsync(Guid osId, string alteradoPor) =>
        AplicarTransicaoAsync(osId, os => os.EnviarParaAprovacao(alteradoPor));

    public Task AprovarAsync(Guid osId, string alteradoPor) =>
        AplicarTransicaoAsync(osId, os => os.Aprovar(alteradoPor));

    public Task RejeitarAsync(Guid osId, string alteradoPor, string motivo) =>
        AplicarTransicaoAsync(osId, os => os.Rejeitar(alteradoPor, motivo));

    public async Task NotificarConclusaoAsync(Guid osId, string alteradoPor)
    {
        var os = await _osRepository.ObterPorIdAsync(osId)
            ?? throw new KeyNotFoundException($"Ordem de serviço {osId} não encontrada.");

        try
        {
            var email = os.Cliente?.Email ?? string.Empty;
            await _notificacaoService.EnviarConclusaoAsync(osId, email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Falha ao enviar notificação de conclusão para OS {OsId}. Status será atualizado mesmo assim.",
                osId);
        }

        os.Finalizar(alteradoPor);
        await _osRepository.UpdateAsync(os);
    }

    public Task EntregarAsync(Guid osId, string alteradoPor) =>
        AplicarTransicaoAsync(osId, os => os.Entregar(alteradoPor));

    public Task ForcarStatusAsync(Guid osId, EnumStatusOS novoStatus, string alteradoPor, string motivo) =>
        AplicarTransicaoAsync(osId, os => os.ForcarStatus(novoStatus, alteradoPor, motivo));

    public async Task<IEnumerable<HistoricoStatusOSDto>> ObterHistoricoAsync(Guid osId)
    {
        var os = await _osRepository.ObterPorIdAsync(osId)
            ?? throw new KeyNotFoundException($"Ordem de serviço {osId} não encontrada.");

        var historicos = await _historicoRepository.ObterPorOSIdAsync(osId);
        return historicos.Select(MapToDto);
    }

    private async Task AplicarTransicaoAsync(Guid osId, Action<OrdemServico> aplicar)
    {
        var os = await _osRepository.ObterPorIdAsync(osId)
            ?? throw new KeyNotFoundException($"Ordem de serviço {osId} não encontrada.");

        aplicar(os);
        await _osRepository.UpdateAsync(os);
    }

    private static HistoricoStatusOSDto MapToDto(HistoricoStatusOS h) => new(
        h.Id,
        h.OrdemServicoId,
        h.StatusAnterior?.ToString(),
        h.StatusNovo.ToString(),
        h.AlteradoEm,
        h.AlteradoPor,
        h.Motivo
    );
}
