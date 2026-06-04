using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Peca.ListarPecasEstoqueBaixo;

public interface IListarPecasEstoqueBaixoUseCase : IUseCase<int, IEnumerable<PecaResponse>> { }
