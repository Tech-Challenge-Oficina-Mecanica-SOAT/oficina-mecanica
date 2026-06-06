using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IOrdemServicoRepository
{
    Task<OrdemServico?> ObterPorIdAsync(Guid id);
    Task<OrdemServico?> ObterPorIdComHistoricoAsync(Guid id);
    Task<OrdemServico?> ObterPorIdComItensAsync(Guid id);
    Task<IEnumerable<OrdemServico>> ListarTodosAsync();
    Task<Guid> CriarAsync(OrdemServico ordemServico);
    Task<IEnumerable<OrdemServicoItem>> AdicionarItensAsync(IEnumerable<OrdemServicoItem> itens);
    Task RemoverItemAsync(Guid ordemServicoId, Guid itemId);
    Task AtualizarTotalAsync(Guid ordemServicoId, decimal total);
    Task<double> GetTempoMedioExecucaoHorasAsync();
    Task UpdateAsync(OrdemServico ordemServico);
    Task<OrdemServico?> ObterPorTokenAsync(string token);
    Task<IEnumerable<OrdemServico>> ListarAtivasOrdenadasAsync();
    Task AtualizarTokenAsync(Guid osId, string token);

}
