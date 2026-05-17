using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.OrdemServico.AdicionarItensOS;

public class AdicionarItensOSUseCase : IAdicionarItensOSUseCase
{
    private readonly IOrdemServicoRepository _repository;
    private readonly OrdemServicoMapper _mapper;
    private readonly IMarcarAguardandoAprovacaoUseCase _marcarAguardandoAprovacao;
    private readonly INotificacaoService _notificacao;

    public AdicionarItensOSUseCase(
        IOrdemServicoRepository repository,
        OrdemServicoMapper mapper,
        IMarcarAguardandoAprovacaoUseCase marcarAguardandoAprovacao,
        INotificacaoService notificacao)
    {
        _repository = repository;
        _mapper = mapper;
        _marcarAguardandoAprovacao = marcarAguardandoAprovacao;
        _notificacao = notificacao;
    }

    public async Task<Result<IEnumerable<OrdemServicoItemResponse>>> ExecutarAsync(AdicionarItensOSRequest request)
    {
        var os = await _repository.ObterPorIdComItensAsync(request.OrdemServicoId);
        if (os is null)
            return Result<IEnumerable<OrdemServicoItemResponse>>.NotFound("Ordem de serviço não encontrada.");

        var itens = new List<OrdemServicoItem>();
        foreach (var itemDto in request.Itens)
        {
            if (!Enum.TryParse<TipoOSItem>(itemDto.Tipo, ignoreCase: true, out var tipo))
                return Result<IEnumerable<OrdemServicoItemResponse>>.Validation("Tipo inválido. Use: servico, peca ou insumo.");

            itens.Add(new OrdemServicoItem(
                request.OrdemServicoId,
                tipo,
                itemDto.ReferenciaId,
                itemDto.Descricao,
                itemDto.Quantidade,
                itemDto.PrecoUnitario));
        }

        var salvos = await _repository.AdicionarItensAsync(itens);

        foreach (var item in salvos)
            if (!os.Itens.Contains(item)) os.Itens.Add(item);

        os.RecalcularTotal();
        await _repository.AtualizarTotalAsync(request.OrdemServicoId, os.Total);

        await _marcarAguardandoAprovacao.ExecutarAsync(new MarcarAguardandoAprovacaoRequest(request.OrdemServicoId, "sistema"));
        await _notificacao.EnviarOrcamentoAsync(request.OrdemServicoId, os.Cliente?.Email ?? string.Empty, os.Total);

        return Result<IEnumerable<OrdemServicoItemResponse>>.Success(salvos.Select(_mapper.MapToItemResponse));
    }
}
