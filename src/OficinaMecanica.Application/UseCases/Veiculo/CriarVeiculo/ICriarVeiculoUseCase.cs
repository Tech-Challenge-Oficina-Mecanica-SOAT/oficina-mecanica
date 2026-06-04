using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Veiculo.CriarVeiculo;

public interface ICriarVeiculoUseCase : IUseCase<CriarVeiculoRequest, VeiculoResponse> { }
