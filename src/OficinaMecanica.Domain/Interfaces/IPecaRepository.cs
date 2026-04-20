using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces
{
    public interface IPecaRepository
    {
        Task<Peca?> GetByIdAsync(Guid id);
        Task<IEnumerable<Peca>> GetAllAsync();
        Task<IEnumerable<Peca>> GetByEstoqueBaixoAsync(int limiteEstoque);
        Task<Peca?> GetByCodigoAsync(string codigo);
        Task<Peca> AddAsync(Peca peca);
        Task<Peca> UpdateAsync(Peca peca);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsByIdAsync(Guid id);
        Task<bool> ExistsByCodigoAsync(string codigo);
        Task<int> GetEstoqueAsync(Guid id);
        Task<bool> DecrementarEstoqueAsync(Guid id, int quantidade);
        Task<bool> IncrementarEstoqueAsync(Guid id, int quantidade);
    }
}
