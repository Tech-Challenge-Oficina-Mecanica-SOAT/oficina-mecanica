using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.Servico.ListarServicosAtivos;

public interface IListarServicosAtivosUseCase : IUseCase<Unit, IEnumerable<ServicoResponse>> { }
