using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.Interfaces;

public interface IVeiculoService
{
    Task<VeiculoResponse?> GetByIdAsync(Guid id);
    Task<VeiculoResponse?> GetByPlacaAsync(string placa);
    Task<IEnumerable<VeiculoResponse>> GetAllAsync();
    Task<IEnumerable<VeiculoResponse>> GetByClienteIdAsync(Guid clienteId);
    Task<VeiculoResponse> CreateAsync(CriarVeiculoRequest createDto);
    Task<VeiculoResponse> UpdateAsync(Guid id, AtualizarVeiculoRequest updateDto);
    Task DeleteAsync(Guid id);
}
