using OficinaMecanica.Application.DTOs;

namespace OficinaMecanica.Application.Interfaces;

public interface IOrdemServicoService
{
    Task<OrdemServicoDto> CreateAsync(CreateOrdemServicoDto createDto);
    Task<OrdemServicoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrdemServicoResumoDto>> GetAllAsync();
    Task<IEnumerable<OrdemServicoItemDto>> AddItensAsync(Guid ordemServicoId, List<CreateOrdemServicoItemDto> itensDto);
    Task RemoveItemAsync(Guid ordemServicoId, Guid itemId);
    Task<double> GetTempoMedioExecucaoAsync(); // em horas
}