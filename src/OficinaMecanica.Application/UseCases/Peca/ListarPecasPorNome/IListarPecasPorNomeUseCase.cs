using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Peca.ListarPecasPorNome;

public interface IListarPecasPorNomeUseCase : IUseCase<string, IEnumerable<PecaResponse>> { }
