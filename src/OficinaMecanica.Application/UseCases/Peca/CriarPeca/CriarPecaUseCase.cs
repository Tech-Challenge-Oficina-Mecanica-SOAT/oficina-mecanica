using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Peca.CriarPeca;

public class CriarPecaUseCase : ICriarPecaUseCase
{
    private readonly IPecaInsumoRepository _repository;
    private readonly PecaMapper _mapper;

    public CriarPecaUseCase(IPecaInsumoRepository repository, PecaMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PecaResponse>> ExecutarAsync(CriarPecaRequest request)
    {
        if (await _repository.ExistsByCodigoAsync(request.Codigo))
            return Result<PecaResponse>.Conflict("Já existe uma peça com este código.");

        try
        {
            var peca = new PecaInsumo(
                request.Nome,
                request.Codigo,
                request.Descricao,
                request.PrecoUnitario,
                request.Estoque);
            var criada = await _repository.AddAsync(peca);
            return Result<PecaResponse>.Success(_mapper.MapToResponse(criada));
        }
        catch (ArgumentException ex)
        {
            return Result<PecaResponse>.Validation(ex.Message);
        }
    }
}
