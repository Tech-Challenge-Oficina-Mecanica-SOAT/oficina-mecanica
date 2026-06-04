using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Peca.ConsultarPeca;

public interface IConsultarPecaUseCase : IUseCase<Guid, PecaResponse> { }
