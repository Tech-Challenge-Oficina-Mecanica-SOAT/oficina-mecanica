using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Servico.CriarServico;

public interface ICriarServicoUseCase : IUseCase<CriarServicoRequest, ServicoResponse> { }
