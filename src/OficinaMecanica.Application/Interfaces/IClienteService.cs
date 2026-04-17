using OficinaMecanica.Application.DTOs;

namespace OficinaMecanica.Application.Interfaces;

public interface IClienteService
{
    Task<ClienteDto?> GetByIdAsync(Guid id);
    Task<ClienteDto?> GetByDocumentoAsync(string documento);
    Task<IEnumerable<ClienteDto>> GetAllAsync();
    Task<ClienteDto> CreateAsync(CreateClienteDto createDto);
    Task<ClienteDto> UpdateAsync(Guid id, UpdateClienteDto updateDto);
    Task DeleteAsync(Guid id);
    Task AtivarAsync(Guid id);
    Task DesativarAsync(Guid id);
}
