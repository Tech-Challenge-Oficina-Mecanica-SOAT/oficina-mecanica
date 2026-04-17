using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IVeiculoRepository
{
    Task<Veiculo?> GetByIdAsync(Guid id);
    Task<Veiculo?> GetByPlacaAsync(string placa);
    Task<IEnumerable<Veiculo>> GetAllAsync();
    Task<IEnumerable<Veiculo>> GetByClienteIdAsync(Guid clienteId);
    Task<Veiculo> AddAsync(Veiculo veiculo);
    Task UpdateAsync(Veiculo veiculo);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsByPlacaAsync(string placa);
    Task<bool> ExistsByPlacaForOtherClienteAsync(string placa, Guid clienteId, Guid? veiculoId = null);
}
