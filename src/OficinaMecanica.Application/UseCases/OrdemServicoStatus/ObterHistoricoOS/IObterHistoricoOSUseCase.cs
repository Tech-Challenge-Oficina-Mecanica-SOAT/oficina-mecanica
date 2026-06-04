using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.ObterHistoricoOS;

public interface IObterHistoricoOSUseCase : IUseCase<Guid, IEnumerable<HistoricoStatusOSResponse>> { }
