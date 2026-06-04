using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Cliente.ConsultarCliente;

public interface IConsultarClienteUseCase : IUseCase<Guid, ClienteResponse> { }
