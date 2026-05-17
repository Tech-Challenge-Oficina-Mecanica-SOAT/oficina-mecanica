using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;

public interface IAbrirOrdemServicoUseCase
    : IUseCase<AbrirOrdemServicoRequest, OrdemServicoResponse> { }
