using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Peca.ListarPecasPorNome;

public class ListarPecasPorNomeUseCase : IListarPecasPorNomeUseCase
{
    private readonly IPecaInsumoRepository _repository;
    private readonly PecaMapper _mapper;

    public ListarPecasPorNomeUseCase(IPecaInsumoRepository repository, PecaMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<PecaResponse>>> ExecutarAsync(string nome)
    {
        var pecas = await _repository.GetByNomeAsync(nome);
        return Result<IEnumerable<PecaResponse>>.Success(pecas.Select(_mapper.MapToResponse));
    }
}
