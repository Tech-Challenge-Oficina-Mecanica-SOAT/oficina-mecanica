using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Peca.ConsultarPeca;

public class ConsultarPecaUseCase : IConsultarPecaUseCase
{
    private readonly IPecaInsumoRepository _repository;
    private readonly PecaMapper _mapper;

    public ConsultarPecaUseCase(IPecaInsumoRepository repository, PecaMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PecaResponse>> ExecutarAsync(Guid id)
    {
        var peca = await _repository.GetByIdAsync(id);
        return peca is null
            ? Result<PecaResponse>.NotFound("Peça não encontrada.")
            : Result<PecaResponse>.Success(_mapper.MapToResponse(peca));
    }
}
