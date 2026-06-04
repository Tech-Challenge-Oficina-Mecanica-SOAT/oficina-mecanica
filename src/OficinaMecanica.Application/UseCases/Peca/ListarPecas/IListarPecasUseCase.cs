using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Peca.ListarPecas;

public interface IListarPecasUseCase : IUseCase<Unit, IEnumerable<PecaResponse>> { }
