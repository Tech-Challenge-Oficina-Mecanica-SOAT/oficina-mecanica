using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.RemoverItemOS;

public class RemoverItemOSUseCase : IRemoverItemOSUseCase
{
    private readonly IOrdemServicoRepository _repository;

    public RemoverItemOSUseCase(IOrdemServicoRepository repository) => _repository = repository;

    public async Task<Result<bool>> ExecutarAsync(RemoverItemOSRequest request)
    {
        var os = await _repository.ObterPorIdComItensAsync(request.OrdemServicoId);
        if (os is null)
            return Result<bool>.NotFound("Ordem de serviço não encontrada.");

        if (!os.Itens.Any(i => i.Id == request.ItemId))
            return Result<bool>.NotFound("Item não encontrado nesta ordem de serviço.");

        await _repository.RemoverItemAsync(request.OrdemServicoId, request.ItemId);
        os.Itens = os.Itens.Where(i => i.Id != request.ItemId).ToList();
        os.RecalcularTotal();
        await _repository.AtualizarTotalAsync(request.OrdemServicoId, os.Total);

        return Result<bool>.Success(true);
    }
}
