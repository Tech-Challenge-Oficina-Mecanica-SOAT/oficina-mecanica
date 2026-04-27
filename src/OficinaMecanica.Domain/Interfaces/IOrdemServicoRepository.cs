using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IOrdemServicoRepository
{
    Task<OrdemServico?> ObterPorIdAsync(Guid id);
    Task<OrdemServico?> ObterPorIdComHistoricoAsync(Guid id);
    Task UpdateAsync(OrdemServico ordemServico);
}
