using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;

public class ListarOrdensServicoUseCase : IListarOrdensServicoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;

    public ListarOrdensServicoUseCase(IOrdemServicoRepository repository, OrdemServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<OrdemServicoResumoResponse>>> ExecutarAsync(Unit _)
    {
        var lista = await _repository.ListarTodosAsync();
        return Result<IEnumerable<OrdemServicoResumoResponse>>.Success(lista.Select(_mapper.MapToResumoResponse));
    }
}
