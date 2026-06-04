using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Servico.ConsultarServico;

public class ConsultarServicoUseCase : IConsultarServicoUseCase
{
    private readonly IServicoRepository _repository;
    private readonly ServicoMapper _mapper;

    public ConsultarServicoUseCase(IServicoRepository repository, ServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ServicoResponse>> ExecutarAsync(Guid id)
    {
        var servico = await _repository.GetByIdAsync(id);
        if (servico is null)
            return Result<ServicoResponse>.NotFound("Serviço não encontrado.");

        return Result<ServicoResponse>.Success(_mapper.MapToResponse(servico));
    }
}
