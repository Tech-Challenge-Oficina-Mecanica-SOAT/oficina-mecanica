using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Peca.CriarPeca;

public interface ICriarPecaUseCase : IUseCase<CriarPecaRequest, PecaResponse> { }
