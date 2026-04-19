using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IServicoRepository
{
    Task<Servico?> GetByIdAsync(Guid id);
    Task<IEnumerable<Servico>> GetByNomeAsync(string nome);
    Task<IEnumerable<Servico>> GetAllAsync();
    Task<IEnumerable<Servico>> GetAtivosAsync();
    Task<Servico> AddAsync(Servico servico);
    Task UpdateAsync(Servico servico);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsByNomeAsync(string nome);
}
