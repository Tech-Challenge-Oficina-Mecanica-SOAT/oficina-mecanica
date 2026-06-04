using OficinaMecanica.Application.Common;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Servico.DesativarServico;

public class DesativarServicoUseCase : IDesativarServicoUseCase
{
    private readonly IServicoRepository _repository;

    public DesativarServicoUseCase(IServicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> ExecutarAsync(Guid id)
    {
        var servico = await _repository.GetByIdAsync(id);
        if (servico is null)
            return Result<bool>.NotFound("Serviço não encontrado.");

        servico.Desativar();
        await _repository.UpdateAsync(servico);
        return Result<bool>.Success(true);
    }
}
