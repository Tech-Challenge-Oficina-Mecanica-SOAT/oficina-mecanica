using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Repositories;

public class ServicoRepository : IServicoRepository
{
    private readonly ApplicationDbContext _context;

    public ServicoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Servico?> GetByIdAsync(Guid id)
    {
        return await _context.Servicos.FindAsync(id);
    }

    public async Task<IEnumerable<Servico>> GetByNomeAsync(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return await GetAllAsync();

        return await _context.Servicos
            .Where(s => EF.Functions.ILike(s.Nome, $"%{nome}%"))
            .OrderBy(s => s.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Servico>> GetAllAsync()
    {
        return await _context.Servicos.ToListAsync();
    }

    public async Task<IEnumerable<Servico>> GetAtivosAsync()
    {
        return await _context.Servicos.Where(s => s.Ativo).ToListAsync();
    }

    public async Task<Servico> AddAsync(Servico servico)
    {
        await _context.Servicos.AddAsync(servico);
        await _context.SaveChangesAsync();
        return servico;
    }

    public async Task UpdateAsync(Servico servico)
    {
        _context.Entry(servico).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var servico = await GetByIdAsync(id);
        if (servico != null)
        {
            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByNomeAsync(string nome)
    {
        return await _context.Servicos.AnyAsync(s => s.Nome == nome);
    }
}
