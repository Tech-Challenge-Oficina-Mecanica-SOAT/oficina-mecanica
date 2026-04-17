using OficinaMecanica.Application.DTOs;

namespace OficinaMecanica.Application.Interfaces;

public interface IVeiculoService
{
    Task<VeiculoDto?> GetByIdAsync(Guid id);
    Task<VeiculoDto?> GetByPlacaAsync(string placa);
    Task<IEnumerable<VeiculoDto>> GetAllAsync();
    Task<IEnumerable<VeiculoDto>> GetByClienteIdAsync(Guid clienteId);
    Task<VeiculoDto> CreateAsync(CreateVeiculoDto createDto);
    Task<VeiculoDto> UpdateAsync(Guid id, UpdateVeiculoDto updateDto);
    Task DeleteAsync(Guid id);
}
