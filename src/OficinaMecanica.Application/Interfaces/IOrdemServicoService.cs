using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.Interfaces;

public interface IOrdemServicoService
{
    Task<OrdemServicoResponse> CreateAsync(AbrirOrdemServicoRequest createDto);
    Task<OrdemServicoResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrdemServicoResumoResponse>> GetAllAsync();
    Task<IEnumerable<OrdemServicoItemResponse>> AddItensAsync(Guid ordemServicoId, List<AdicionarOSItemRequest> itensDto);
    Task RemoveItemAsync(Guid ordemServicoId, Guid itemId);
    Task<double> GetTempoMedioExecucaoAsync(); // em horas
}