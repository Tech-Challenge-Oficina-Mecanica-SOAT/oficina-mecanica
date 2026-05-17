using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculos;

public interface IListarVeiculosUseCase : IUseCase<Unit, IEnumerable<VeiculoResponse>> { }
