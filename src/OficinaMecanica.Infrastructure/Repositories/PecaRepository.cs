using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Repositories
{
    public class PecaRepository : IPecaRepository
    {
        private readonly ApplicationDbContext _context;

        public PecaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Peca?> GetByIdAsync(Guid id)
        {
            return await _context.Pecas.FindAsync(id);
        }

        public async Task<IEnumerable<Peca>> GetAllAsync()
        {
            return await _context.Pecas
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<Peca>> GetByEstoqueBaixoAsync(int limiteEstoque)
        {
            return await _context.Pecas
                .Where(p => p.Estoque <= limiteEstoque)
                .OrderBy(p => p.Estoque)
                .ToListAsync();
        }

        public async Task<Peca?> GetByCodigoAsync(string codigo)
        {
            return await _context.Pecas
                .FirstOrDefaultAsync(p => p.Codigo == codigo);
        }

        public async Task<Peca> AddAsync(Peca peca)
        {
            peca.Id = Guid.NewGuid();
            peca.CriadoEm = DateTime.UtcNow;
            _context.Pecas.Add(peca);
            await _context.SaveChangesAsync();
            return peca;
        }

        public async Task<Peca> UpdateAsync(Peca peca)
        {
            peca.AtualizadoEm = DateTime.UtcNow;
            _context.Pecas.Update(peca);
            await _context.SaveChangesAsync();
            return peca;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var peca = await _context.Pecas.FindAsync(id);
            if (peca == null) return false;
            
            _context.Pecas.Remove(peca);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByIdAsync(Guid id)
        {
            return await _context.Pecas.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExistsByCodigoAsync(string codigo)
        {
            return await _context.Pecas.AnyAsync(p => p.Codigo == codigo);
        }

        public async Task<int> GetEstoqueAsync(Guid id)
        {
            var peca = await _context.Pecas.FindAsync(id);
            return peca?.Estoque ?? 0;
        }

        public async Task<bool> DecrementarEstoqueAsync(Guid id, int quantidade)
        {
            var peca = await _context.Pecas.FindAsync(id);
            if (peca == null || peca.Estoque < quantidade) return false;
            
            peca.Estoque -= quantidade;
            peca.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementarEstoqueAsync(Guid id, int quantidade)
        {
            var peca = await _context.Pecas.FindAsync(id);
            if (peca == null) return false;
            
            peca.Estoque += quantidade;
            peca.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
