using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Peca.ListarPecasEstoqueBaixo;

public class ListarPecasEstoqueBaixoUseCase : IListarPecasEstoqueBaixoUseCase
{
    private readonly IPecaInsumoRepository _repository;
    private readonly PecaMapper _mapper;

    public ListarPecasEstoqueBaixoUseCase(IPecaInsumoRepository repository, PecaMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<PecaResponse>>> ExecutarAsync(int limiteEstoque)
    {
        var pecas = await _repository.GetByEstoqueBaixoAsync(limiteEstoque);
        return Result<IEnumerable<PecaResponse>>.Success(pecas.Select(_mapper.MapToResponse));
    }
}
