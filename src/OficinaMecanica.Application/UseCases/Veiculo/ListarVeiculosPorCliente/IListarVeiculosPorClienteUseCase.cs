using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculosPorCliente;

public interface IListarVeiculosPorClienteUseCase : IUseCase<Guid, IEnumerable<VeiculoResponse>> { }
