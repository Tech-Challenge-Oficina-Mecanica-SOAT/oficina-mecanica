using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Servico.AtivarServico;

public class AtivarServicoUseCase : IAtivarServicoUseCase
{
    private readonly IServicoRepository _repository;

    public AtivarServicoUseCase(IServicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        var servico = await _repository.GetByIdAsync(id);
        if (servico is null)
            return Result<bool>.NotFound("Serviço não encontrado.");

        servico.Ativar();
        await _repository.UpdateAsync(servico);
        return Result<bool>.Success(true);
    }
}
