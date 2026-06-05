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

    public async Task<IEnumerable<OrdemServico>> ListarTodosAsync()
    {
        var query = _context.OrdensServico
            .Include(os => os.Cliente)
            .Include(os => os.Veiculo)
            .Include(os => os.Itens)
            .Where(os => os.StatusOS != EnumStatusOS.Finalizada && 
                        os.StatusOS != EnumStatusOS.Entregue)
            .OrderBy(os => os.StatusOS == EnumStatusOS.EmExecucao ? 1 :
                        os.StatusOS == EnumStatusOS.AguardandoAprovacao ? 2 :
                        os.StatusOS == EnumStatusOS.EmDiagnostico ? 3 :
                        os.StatusOS == EnumStatusOS.Recebida ? 4 : 5)
            .ThenBy(os => os.DataAbertura);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<OrdemServico>> ListarAtivasOrdenadasAsync()
    {
        // Mapeamento dos status para prioridade
        // EmExecucao (4) = 1ª prioridade
        // AguardandoAprovacao (3) = 2ª prioridade
        // EmDiagnostico (2) = 3ª prioridade
        // Recebida (1) = 4ª prioridade
        
        var query = _context.OrdensServico
            .Include(os => os.Cliente)
            .Include(os => os.Veiculo)
            .Include(os => os.Itens)
            .Where(os => os.StatusOS != EnumStatusOS.Finalizada && 
                        os.StatusOS != EnumStatusOS.Entregue &&
                        os.StatusOS != EnumStatusOS.Rejeitada)
            .ToList()
            .OrderBy(os => os.StatusOS switch
            {
                EnumStatusOS.EmExecucao => 1,
                EnumStatusOS.AguardandoAprovacao => 2,
                EnumStatusOS.EmDiagnostico => 3,
                EnumStatusOS.Recebida => 4,
                _ => 5
            })
            .ThenBy(os => os.DataAbertura);

        return query;
    }

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
