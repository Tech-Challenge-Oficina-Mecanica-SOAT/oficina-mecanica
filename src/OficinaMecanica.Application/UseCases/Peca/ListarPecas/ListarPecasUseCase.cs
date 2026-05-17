using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Peca.ListarPecas;

public class ListarPecasUseCase : IListarPecasUseCase
{
    private readonly IPecaInsumoRepository _repository;
    private readonly PecaMapper _mapper;

    public ListarPecasUseCase(IPecaInsumoRepository repository, PecaMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<PecaResponse>>> ExecutarAsync(Unit _)
    {
        var pecas = await _repository.GetAllAsync();
        return Result<IEnumerable<PecaResponse>>.Success(pecas.Select(_mapper.MapToResponse));
    }
}
