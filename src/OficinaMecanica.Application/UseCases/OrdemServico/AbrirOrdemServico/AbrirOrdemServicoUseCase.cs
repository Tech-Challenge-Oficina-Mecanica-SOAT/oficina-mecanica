using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;

public class AbrirOrdemServicoUseCase : IAbrirOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;

    public AbrirOrdemServicoUseCase(IOrdemServicoRepository repository, OrdemServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrdemServicoResponse>> ExecutarAsync(AbrirOrdemServicoRequest request)
    {
        if (request.ClienteId == Guid.Empty)
            return Result<OrdemServicoResponse>.Validation("ClienteId é obrigatório.");

        if (request.VeiculoId == Guid.Empty)
            return Result<OrdemServicoResponse>.Validation("VeiculoId é obrigatório.");

        var os = new Domain.Entities.OrdemServico(request.ClienteId, request.VeiculoId, request.Observacoes);
        var id = await _repository.CriarAsync(os);
        var criada = await _repository.ObterPorIdComItensAsync(id);

        return Result<OrdemServicoResponse>.Success(_mapper.MapToResponse(criada!));
    }
}
