using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Servico.ListarServicosPorNome;

public class ListarServicosPorNomeUseCase : IListarServicosPorNomeUseCase
{
    private readonly IServicoRepository _repository;
    private readonly ServicoMapper _mapper;

    public ListarServicosPorNomeUseCase(IServicoRepository repository, ServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ServicoResponse>>> ExecutarAsync(string nome)
    {
        var servicos = await _repository.GetByNomeAsync(nome);
        return Result<IEnumerable<ServicoResponse>>.Success(servicos.Select(_mapper.MapToResponse));
    }
}
