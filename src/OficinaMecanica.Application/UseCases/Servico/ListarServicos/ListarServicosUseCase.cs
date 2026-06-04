using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Servico.ListarServicos;

public class ListarServicosUseCase : IListarServicosUseCase
{
    private readonly IServicoRepository _repository;
    private readonly ServicoMapper _mapper;

    public ListarServicosUseCase(IServicoRepository repository, ServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ServicoResponse>>> ExecutarAsync(Unit _)
    {
        var servicos = await _repository.GetAllAsync();
        return Result<IEnumerable<ServicoResponse>>.Success(servicos.Select(_mapper.MapToResponse));
    }
}
