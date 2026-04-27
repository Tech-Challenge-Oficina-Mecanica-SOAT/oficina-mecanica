using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Repositories;

public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly ApplicationDbContext _context;

    public OrdemServicoRepository(ApplicationDbContext context) => _context = context;

    public async Task<OrdemServico?> ObterPorIdAsync(Guid id) =>
        await _context.OrdensServico
            .Include(o => o.Cliente)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<OrdemServico?> ObterPorIdComHistoricoAsync(Guid id) =>
        await _context.OrdensServico
            .Include(o => o.Cliente)
            .Include(o => o.Historico)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task UpdateAsync(OrdemServico ordemServico)
    {
        foreach (var historico in ordemServico.Historico)
        {
            var historicoEntry = _context.Entry(historico);
            if (historicoEntry.State == EntityState.Detached)
                _context.HistoricosStatusOS.Add(historico);
        }

        await _context.SaveChangesAsync();
    }
}
