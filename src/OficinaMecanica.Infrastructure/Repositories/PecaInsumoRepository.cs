using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Repositories;

public class PecaInsumoRepository : IPecaInsumoRepository
{
    private readonly ApplicationDbContext _context;

    public PecaInsumoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PecaInsumo?> GetByIdAsync(Guid id)
    {
        return await _context.PecasInsumos.FindAsync(id);
    }

    public async Task<PecaInsumo?> GetByCodigoAsync(string codigo)
    {
        return await _context.PecasInsumos.FirstOrDefaultAsync(p => p.Codigo == codigo);
    }

    public async Task<IEnumerable<PecaInsumo>> GetAllAsync()
    {
        return await _context.PecasInsumos.ToListAsync();
    }

    public async Task<IEnumerable<PecaInsumo>> GetByNomeAsync(string nome)
    {
        return await _context.PecasInsumos
            .Where(p => EF.Functions.ILike(p.Nome, $"%{nome}%"))
            .ToListAsync();
    }

    public async Task<PecaInsumo> AddAsync(PecaInsumo peca)
    {
        await _context.PecasInsumos.AddAsync(peca);
        await _context.SaveChangesAsync();
        return peca;
    }

    public async Task<PecaInsumo> UpdateAsync(PecaInsumo peca)
    {
        _context.Entry(peca).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return peca;
    }

    public async Task DeleteAsync(Guid id)
    {
        var peca = await GetByIdAsync(id);
        if (peca != null)
        {
            _context.PecasInsumos.Remove(peca);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByCodigoAsync(string codigo)
    {
        return await _context.PecasInsumos.AnyAsync(p => p.Codigo == codigo);
    }

    public async Task<int> GetEstoqueAsync(Guid id)
    {
        var peca = await GetByIdAsync(id);
        return peca?.Quantidade ?? 0;
    }

    public async Task<PecaInsumo> IncrementarEstoqueAsync(Guid id, int quantidade)
    {
        var peca = await GetByIdAsync(id);
        if (peca == null) throw new KeyNotFoundException("Peça não encontrada");

        peca.IncrementarEstoque(quantidade);
        await UpdateAsync(peca);
        return peca;
    }

    public async Task<PecaInsumo> DecrementarEstoqueAsync(Guid id, int quantidade)
    {
        var peca = await GetByIdAsync(id);
        if (peca == null) throw new KeyNotFoundException("Peça não encontrada");

        peca.DecrementarEstoque(quantidade);
        await UpdateAsync(peca);
        return peca;
    }

    public async Task<IEnumerable<PecaInsumo>> GetByEstoqueBaixoAsync(int limiteEstoque)
    {
        return await _context.PecasInsumos
            .Where(p => p.Quantidade <= limiteEstoque)
            .ToListAsync();
    }
}
