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

    public async Task<OrdemServico?> ObterPorIdComItensAsync(Guid id) =>
        await _context.OrdensServico
            .Include(os => os.Cliente)
            .Include(os => os.Veiculo)
            .Include(os => os.Itens)
            .FirstOrDefaultAsync(os => os.Id == id);

    public async Task<IEnumerable<OrdemServico>> ListarTodosAsync() =>
        await _context.OrdensServico
            .Include(os => os.Cliente)
            .Include(os => os.Veiculo)
            .Include(os => os.Itens)
            .OrderByDescending(os => os.DataAbertura)
            .ToListAsync();

    public async Task<Guid> CriarAsync(OrdemServico ordemServico)
    {
        _context.OrdensServico.Add(ordemServico);
        await _context.SaveChangesAsync();
        return ordemServico.Id;
    }

    public async Task<IEnumerable<OrdemServicoItem>> AdicionarItensAsync(IEnumerable<OrdemServicoItem> itens)
    {
        _context.OrdensServicoItens.AddRange(itens);
        await _context.SaveChangesAsync();
        return itens;
    }

    public async Task RemoverItemAsync(Guid ordemServicoId, Guid itemId)
    {
        var item = await _context.OrdensServicoItens
            .FirstOrDefaultAsync(i => i.Id == itemId && i.OrdemServicoId == ordemServicoId)
            ?? throw new KeyNotFoundException("Item não encontrado");

        _context.OrdensServicoItens.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarTotalAsync(Guid ordemServicoId, decimal total)
    {
        var os = await _context.OrdensServico.FindAsync(ordemServicoId)
            ?? throw new KeyNotFoundException("Ordem de serviço não encontrada");

        os.Total = total;
        await _context.SaveChangesAsync();
    }

    public async Task<double> GetTempoMedioExecucaoHorasAsync()
    {
        var osFinalizadas = await _context.OrdensServico
            .Where(os => os.DataFechamento.HasValue)
            .ToListAsync();

        if (!osFinalizadas.Any())
            return 0;

        var mediaHoras = osFinalizadas
            .Average(os => (os.DataFechamento!.Value - os.DataAbertura).TotalHours);

        return Math.Round(mediaHoras, 2);
    }

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
