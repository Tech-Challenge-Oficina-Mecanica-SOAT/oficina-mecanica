using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Peca.AtualizarPeca;

public class AtualizarPecaUseCase : IAtualizarPecaUseCase
{
    private readonly IPecaInsumoRepository _repository;
    private readonly PecaMapper _mapper;

    public AtualizarPecaUseCase(IPecaInsumoRepository repository, PecaMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PecaResponse>> ExecutarAsync(AtualizarPecaUseCaseRequest request)
    {
        var peca = await _repository.GetByIdAsync(request.Id);
        if (peca is null)
            return Result<PecaResponse>.NotFound("Peça não encontrada.");

        try
        {
            peca.Atualizar(request.Nome, request.Descricao, request.PrecoUnitario, request.Estoque);
        }
        catch (ArgumentException ex)
        {
            return Result<PecaResponse>.Validation(ex.Message);
        }

        var updated = await _repository.UpdateAsync(peca);
        return Result<PecaResponse>.Success(_mapper.MapToResponse(updated));
    }
}
