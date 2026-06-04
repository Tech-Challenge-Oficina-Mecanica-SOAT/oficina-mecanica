using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Veiculo.ConsultarVeiculoPorPlaca;

public interface IConsultarVeiculoPorPlacaUseCase : IUseCase<string, VeiculoResponse> { }
