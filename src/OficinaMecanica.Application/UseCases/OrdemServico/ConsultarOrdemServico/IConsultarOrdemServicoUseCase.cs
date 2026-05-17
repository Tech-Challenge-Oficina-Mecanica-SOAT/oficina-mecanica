using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ConsultarOrdemServico;

public interface IConsultarOrdemServicoUseCase
    : IUseCase<Guid, OrdemServicoResponse> { }
