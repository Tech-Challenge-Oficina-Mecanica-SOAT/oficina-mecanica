using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Interfaces;

public interface IOrdemServicoStatusService
{
    Task IniciarDiagnosticoAsync(Guid osId, string alteradoPor);
    Task MarcarAguardandoAprovacaoAsync(Guid osId, string alteradoPor);
    Task AprovarAsync(Guid osId, string alteradoPor);
    Task RejeitarAsync(Guid osId, string alteradoPor, string motivo);
    Task NotificarConclusaoAsync(Guid osId, string alteradoPor);
    Task EntregarAsync(Guid osId, string alteradoPor);
    Task ForcarStatusAsync(Guid osId, EnumStatusOS novoStatus, string alteradoPor, string motivo);
    Task<IEnumerable<HistoricoStatusOSDto>> ObterHistoricoAsync(Guid osId);
}
