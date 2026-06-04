using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.ConsultarOrdemServico;

public class ConsultarOrdemServicoUseCase : IConsultarOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;

    public ConsultarOrdemServicoUseCase(IOrdemServicoRepository repository, OrdemServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrdemServicoResponse>> ExecutarAsync(Guid id)
    {
        var os = await _repository.ObterPorIdComItensAsync(id);
        if (os is null)
            return Result<OrdemServicoResponse>.NotFound("Ordem de serviço não encontrada.");

        return Result<OrdemServicoResponse>.Success(_mapper.MapToResponse(os));
    }
}
