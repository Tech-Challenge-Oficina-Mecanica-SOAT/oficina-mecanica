using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Repositories;

public class HistoricoStatusOSRepository : IHistoricoStatusOSRepository
{
    private readonly ApplicationDbContext _context;

    public HistoricoStatusOSRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<HistoricoStatusOS>> ObterPorOSIdAsync(Guid ordemServicoId) =>
        await _context.HistoricosStatusOS
            .AsNoTracking()
            .Where(h => h.OrdemServicoId == ordemServicoId)
            .OrderBy(h => h.AlteradoEm)
            .ToListAsync();
}
