using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Servico.ListarServicos;

public interface IListarServicosUseCase : IUseCase<Unit, IEnumerable<ServicoResponse>> { }
