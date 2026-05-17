using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.Interfaces;

public interface IClienteService
{
    Task<ClienteResponse?> GetByIdAsync(Guid id);
    Task<ClienteResponse?> GetByDocumentoAsync(string documento);
    Task<IEnumerable<ClienteResponse>> GetAllAsync();
    Task<ClienteResponse> CreateAsync(CriarClienteRequest createDto);
    Task<ClienteResponse> UpdateAsync(Guid id, AtualizarClienteRequest updateDto);
    Task DeleteAsync(Guid id);
    Task AtivarAsync(Guid id);
    Task DesativarAsync(Guid id);
}
