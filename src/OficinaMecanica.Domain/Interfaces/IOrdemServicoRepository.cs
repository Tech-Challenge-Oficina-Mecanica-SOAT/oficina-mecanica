using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IOrdemServicoRepository
{
    Task<OrdemServico?> ObterPorIdAsync(Guid id);
    Task<OrdemServico?> ObterPorIdComItensAsync(Guid id);
    Task<IEnumerable<OrdemServico>> ListarTodosAsync();
    Task<Guid> CriarAsync(OrdemServico ordemServico);
    Task<OrdemServicoItem> AdicionarItemAsync(OrdemServicoItem item);
    Task RemoverItemAsync(Guid ordemServicoId, Guid itemId);
    Task AtualizarTotalAsync(Guid ordemServicoId, decimal total);
    Task<double> GetTempoMedioExecucaoHorasAsync();
}
