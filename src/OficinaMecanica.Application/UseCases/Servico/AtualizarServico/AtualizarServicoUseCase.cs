using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Servico.AtualizarServico;

public class AtualizarServicoUseCase : IAtualizarServicoUseCase
{
    private readonly IServicoRepository _repository;
    private readonly ServicoMapper _mapper;

    public AtualizarServicoUseCase(IServicoRepository repository, ServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ServicoResponse>> ExecutarAsync(AtualizarServicoUseCaseRequest request)
    {
        var servico = await _repository.GetByIdAsync(request.Id);
        if (servico is null)
            return Result<ServicoResponse>.NotFound("Serviço não encontrado.");

        try
        {
            servico.Atualizar(request.Nome, request.Descricao, request.Valor);
        }
        catch (ArgumentException ex)
        {
            return Result<ServicoResponse>.Validation(ex.Message);
        }

        await _repository.UpdateAsync(servico);
        return Result<ServicoResponse>.Success(_mapper.MapToResponse(servico));
    }
}
