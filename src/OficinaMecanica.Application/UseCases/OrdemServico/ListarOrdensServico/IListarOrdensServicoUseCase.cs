using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;

public interface IListarOrdensServicoUseCase : IUseCase<Unit, IEnumerable<OrdemServicoResumoResponse>>
{
    Task<Result<IEnumerable<OrdemServicoResumoResponse>>> ExecutarAsync(Unit _);
    Task<Result<IEnumerable<OrdemServicoResumoResponse>>> ListarAtivasOrdenadasAsync(Unit _); // NOVO
}