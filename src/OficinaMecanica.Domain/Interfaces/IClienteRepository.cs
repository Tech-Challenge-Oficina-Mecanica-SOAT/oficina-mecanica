using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(Guid id);
    Task<Cliente?> GetByDocumentoAsync(string documento);
    Task<IEnumerable<Cliente>> GetAllAsync();
    Task<Cliente> AddAsync(Cliente cliente);
    Task UpdateAsync(Cliente cliente);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsByDocumentoAsync(string documento);
}
