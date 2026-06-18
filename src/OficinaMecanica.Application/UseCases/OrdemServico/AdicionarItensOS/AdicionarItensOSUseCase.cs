using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
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
    private readonly IPecaInsumoRepository _pecaInsumoRepository;
    private readonly IServicoRepository _servicoRepository;

    public AdicionarItensOSUseCase(
        IOrdemServicoRepository repository,
        OrdemServicoMapper mapper,
        IMarcarAguardandoAprovacaoUseCase marcarAguardandoAprovacao,
        IPecaInsumoRepository pecaInsumoRepository,
        IServicoRepository servicoRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _marcarAguardandoAprovacao = marcarAguardandoAprovacao;
        _pecaInsumoRepository = pecaInsumoRepository;
        _servicoRepository = servicoRepository;
    }

    public async Task<Result<IEnumerable<OrdemServicoItemResponse>>> ExecutarAsync(AdicionarItensOSRequest request)
    {
        var os = await _repository.ObterPorIdComItensAsync(request.OrdemServicoId);
        if (os is null)
            return Result<IEnumerable<OrdemServicoItemResponse>>.NotFound("Ordem de serviço não encontrada.");

        var itens = new List<OrdemServicoItem>();
        var errosValidacao = new List<string>();
        var pecasInsumosAtualizados = new List<PecaInsumo>();

        try
        {
            foreach (var itemDto in request.Itens)
            {
                if (!Enum.TryParse<TipoOSItem>(itemDto.Tipo, ignoreCase: true, out var tipo))
                {
                    errosValidacao.Add($"Tipo inválido para o item. Use: servico, peca ou insumo.");
                    continue;
                }

                if (tipo == TipoOSItem.Peca || tipo == TipoOSItem.Insumo)
                {
                    // Verificar se a referência existe usando GetByIdAsync
                    var pecaInsumo = await _pecaInsumoRepository.GetByIdAsync(itemDto.ReferenciaId);
                    
                    if (pecaInsumo is null)
                    {
                        errosValidacao.Add($"Referência ID {itemDto.ReferenciaId} não encontrada na tabela de Peças/Insumos.");
                        continue;
                    }

                    // Verificar se tem quantidade disponível no estoque
                    var estoqueDisponivel = await _pecaInsumoRepository.GetEstoqueAsync(itemDto.ReferenciaId);
                    
                    if (estoqueDisponivel < itemDto.Quantidade)
                    {
                        errosValidacao.Add($"Estoque insuficiente para o item '{pecaInsumo.Nome}'. " +
                                          $"Disponível: {estoqueDisponivel}, Solicitado: {itemDto.Quantidade}.");
                        continue;
                    }

                    // Subtrair do estoque usando o método DecrementarEstoqueAsync
                    var pecaAtualizada = await _pecaInsumoRepository.DecrementarEstoqueAsync(itemDto.ReferenciaId, itemDto.Quantidade);
                    pecasInsumosAtualizados.Add(pecaAtualizada);

                    // Criar item com informações da tabela de peças/insumos
                    itens.Add(new OrdemServicoItem(
                        request.OrdemServicoId,
                        tipo,
                        itemDto.ReferenciaId,
                        pecaInsumo.Descricao,
                        itemDto.Quantidade,
                        pecaInsumo.Preco
                    ));
                }
                else if (tipo == TipoOSItem.Servico)
                {
                    var servico = await _servicoRepository.GetByIdAsync(itemDto.ReferenciaId);
                    
                    if (servico is null)
                    {
                        errosValidacao.Add($"Referência ID {itemDto.ReferenciaId} não encontrada na tabela de Serviços.");
                        continue;
                    }

                    if (!servico.Ativo)
                    {
                        errosValidacao.Add($"Serviço '{servico.Nome}' está inativo e não pode ser adicionado.");
                        continue;
                    }

                    // Criar item com informações da tabela de serviços
                    // A quantidade é a informada pelo usuário para multiplicar no cálculo do total
                    itens.Add(new OrdemServicoItem(
                        request.OrdemServicoId,
                        tipo,
                        itemDto.ReferenciaId,
                        servico.Descricao,
                        itemDto.Quantidade,
                        servico.Valor
                    ));
                }
                else
                {
                    errosValidacao.Add($"Tipo '{tipo}' não é válido.");
                    continue;
                }
            }

            // Se houver erros, retornar todos eles
            if (errosValidacao.Any())
            {
                // Fazer rollback do estoque se necessário
                foreach (var pecaInsumo in pecasInsumosAtualizados)
                {
                    await _pecaInsumoRepository.IncrementarEstoqueAsync(pecaInsumo.Id, pecaInsumo.Quantidade);
                }
                
                return Result<IEnumerable<OrdemServicoItemResponse>>.Validation(
                    string.Join("; ", errosValidacao));
            }

            // Se não houver itens para adicionar
            if (!itens.Any())
            {
                return Result<IEnumerable<OrdemServicoItemResponse>>.Validation(
                    "Nenhum item válido para adicionar.");
            }

            var salvos = await _repository.AdicionarItensAsync(itens);

            foreach (var item in salvos)
                if (!os.Itens.Contains(item)) os.Itens.Add(item);

            os.RecalcularTotal();
            await _repository.AtualizarTotalAsync(request.OrdemServicoId, os.Total);

            var marcarResult = await _marcarAguardandoAprovacao.ExecutarAsync(
                new MarcarAguardandoAprovacaoRequest(request.OrdemServicoId, "sistema"));
            
            if (!marcarResult.IsSuccess)
            {
                // Fazer rollback do estoque se falhar na transição de status
                foreach (var pecaInsumo in pecasInsumosAtualizados)
                {
                    await _pecaInsumoRepository.IncrementarEstoqueAsync(pecaInsumo.Id, pecaInsumo.Quantidade);
                }
                
                return Result<IEnumerable<OrdemServicoItemResponse>>.Validation(marcarResult.Error ?? "Falha na transição de status.");
            }

            return Result<IEnumerable<OrdemServicoItemResponse>>.Success(salvos.Select(_mapper.MapToItemResponse));
        }
        catch (Exception ex)
        {
            // Fazer rollback do estoque em caso de erro
            foreach (var pecaInsumo in pecasInsumosAtualizados)
            {
                try
                {
                    await _pecaInsumoRepository.IncrementarEstoqueAsync(pecaInsumo.Id, pecaInsumo.Quantidade);
                }
                catch
                {
                    // Log do erro de rollback
                }
            }
            
            return Result<IEnumerable<OrdemServicoItemResponse>>.Validation($"Erro ao processar: {ex.Message}");
        }
    }
}