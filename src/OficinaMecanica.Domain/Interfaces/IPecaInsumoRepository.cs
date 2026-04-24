using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IPecaInsumoRepository
{
    Task<PecaInsumo?> GetByIdAsync(Guid id);
    Task<PecaInsumo?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<PecaInsumo>> GetAllAsync();
    Task<IEnumerable<PecaInsumo>> GetByNomeAsync(string nome);
    Task<PecaInsumo> AddAsync(PecaInsumo peca);
    Task<PecaInsumo> UpdateAsync(PecaInsumo peca);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsByCodigoAsync(string codigo);
    Task<int> GetEstoqueAsync(Guid id);
    Task<PecaInsumo> IncrementarEstoqueAsync(Guid id, int quantidade);
    Task<PecaInsumo> DecrementarEstoqueAsync(Guid id, int quantidade);
    Task<IEnumerable<PecaInsumo>> GetByEstoqueBaixoAsync(int limiteEstoque);
}
