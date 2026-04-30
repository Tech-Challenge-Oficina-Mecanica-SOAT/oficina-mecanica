using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IHistoricoStatusOSRepository
{
    Task<IEnumerable<HistoricoStatusOS>> ObterPorOSIdAsync(Guid ordemServicoId);
}
