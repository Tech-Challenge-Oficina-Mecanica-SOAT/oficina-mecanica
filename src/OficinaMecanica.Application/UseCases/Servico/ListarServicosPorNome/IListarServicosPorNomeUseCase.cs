using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Servico.ListarServicosPorNome;

public interface IListarServicosPorNomeUseCase : IUseCase<string, IEnumerable<ServicoResponse>> { }
