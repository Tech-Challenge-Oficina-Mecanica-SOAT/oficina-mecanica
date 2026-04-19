using OficinaMecanica.Application.DTOs;

namespace OficinaMecanica.Application.Interfaces;

public interface IServicoService
{
    Task<ServicoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServicoDto>> GetByNomeAsync(string nome);
    Task<IEnumerable<ServicoDto>> GetAllAsync();
    Task<IEnumerable<ServicoDto>> GetAtivosAsync();
    Task<ServicoDto> CreateAsync(CreateServicoDto createDto);
    Task<ServicoDto> UpdateAsync(Guid id, UpdateServicoDto updateDto);
    Task DeleteAsync(Guid id);
    Task AtivarAsync(Guid id);
    Task DesativarAsync(Guid id);
}
