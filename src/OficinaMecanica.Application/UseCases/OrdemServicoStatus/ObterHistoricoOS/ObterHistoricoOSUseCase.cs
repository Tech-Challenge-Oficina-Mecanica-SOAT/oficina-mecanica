using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.ObterHistoricoOS;

public class ObterHistoricoOSUseCase : IObterHistoricoOSUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly HistoricoStatusOSMapper _mapper;

    public ObterHistoricoOSUseCase(IOrdemServicoRepository repository, HistoricoStatusOSMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<HistoricoStatusOSResponse>>> ExecutarAsync(Guid osId)
    {
        var os = await _repository.ObterPorIdComHistoricoAsync(osId);
        if (os is null)
            return Result<IEnumerable<HistoricoStatusOSResponse>>.NotFound("Ordem de serviço não encontrada.");

        var resposta = os.Historico.OrderBy(h => h.AlteradoEm).Select(_mapper.MapToResponse);
        return Result<IEnumerable<HistoricoStatusOSResponse>>.Success(resposta);
    }
}
