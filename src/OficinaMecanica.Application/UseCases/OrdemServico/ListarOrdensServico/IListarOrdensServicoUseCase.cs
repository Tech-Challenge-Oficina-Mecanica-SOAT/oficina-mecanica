using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;

public interface IListarOrdensServicoUseCase
    : IUseCase<Unit, IEnumerable<OrdemServicoResumoResponse>> { }
