using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Cliente.ListarClientes;

public interface IListarClientesUseCase : IUseCase<Unit, IEnumerable<ClienteResponse>> { }
