using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Servico.CriarServico;

public class CriarServicoUseCase : ICriarServicoUseCase
{
    private readonly IServicoRepository _repository;
    private readonly ServicoMapper _mapper;

    public CriarServicoUseCase(IServicoRepository repository, ServicoMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ServicoResponse>> ExecutarAsync(CriarServicoRequest request)
    {
        if (await _repository.ExistsByNomeAsync(request.Nome))
            return Result<ServicoResponse>.Conflict("Serviço com este nome já cadastrado.");

        try
        {
            var servico = new Domain.Entities.Servico(request.Nome, request.Descricao, request.Valor);
            var criado = await _repository.AddAsync(servico);
            return Result<ServicoResponse>.Success(_mapper.MapToResponse(criado));
        }
        catch (ArgumentException ex)
        {
            return Result<ServicoResponse>.Validation(ex.Message);
        }
    }
}
