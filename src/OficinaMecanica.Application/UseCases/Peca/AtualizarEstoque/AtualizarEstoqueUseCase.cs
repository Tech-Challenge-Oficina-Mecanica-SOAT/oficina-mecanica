using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Peca.AtualizarEstoque;

public class AtualizarEstoqueUseCase : IAtualizarEstoqueUseCase
{
    private readonly IPecaInsumoRepository _repository;
    private readonly PecaMapper _mapper;

    public AtualizarEstoqueUseCase(IPecaInsumoRepository repository, PecaMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PecaResponse>> ExecutarAsync(AtualizarEstoqueUseCaseRequest request)
    {
        var existente = await _repository.GetByIdAsync(request.Id);
        if (existente is null)
            return Result<PecaResponse>.NotFound($"Peça com ID {request.Id} não encontrada");

        PecaInsumo peca;
        try
        {
            if (request.TipoOperacao == "incrementar")
            {
                peca = await _repository.IncrementarEstoqueAsync(request.Id, request.Quantidade);
            }
            else if (request.TipoOperacao == "decrementar")
            {
                peca = await _repository.DecrementarEstoqueAsync(request.Id, Math.Abs(request.Quantidade));
            }
            else
            {
                return Result<PecaResponse>.Validation(
                    $"tipoOperacao inválido: '{request.TipoOperacao}'. Use 'incrementar' ou 'decrementar'.");
            }
        }
        catch (InvalidOperationException ex) { return Result<PecaResponse>.Validation(ex.Message); }
        catch (ArgumentException ex) { return Result<PecaResponse>.Validation(ex.Message); }

        return Result<PecaResponse>.Success(_mapper.MapToResponse(peca));
    }
}
