using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Cliente.CriarCliente;

public interface ICriarClienteUseCase : IUseCase<CriarClienteRequest, ClienteResponse> { }
