using OficinaMecanica.Application.DTOs;

namespace OficinaMecanica.Application.Interfaces;

public interface IOrdemServicoService
{
    Task<OrdemServicoDto> CreateAsync(CreateOrdemServicoDto createDto);
    Task<OrdemServicoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrdemServicoResumoDto>> GetAllAsync();
    Task<OrdemServicoItemDto> AddItemAsync(Guid ordemServicoId, CreateOrdemServicoItemDto itemDto);
    Task RemoveItemAsync(Guid ordemServicoId, Guid itemId);
    Task<double> GetTempoMedioExecucaoAsync(); // em horas
}