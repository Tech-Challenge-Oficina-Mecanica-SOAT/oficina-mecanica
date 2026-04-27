using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services;

public class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _repository;

    public OrdemServicoService(IOrdemServicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrdemServicoDto> CreateAsync(CreateOrdemServicoDto createDto)
    {
        if (createDto.ClienteId == Guid.Empty)
            throw new ArgumentException("ClienteId é obrigatório!");

        if (createDto.VeiculoId == Guid.Empty)
            throw new ArgumentException("VeiculoId é obrigatório!");

        var os = new OrdemServico
        {
            Id = Guid.NewGuid(),
            ClienteId = createDto.ClienteId,
            VeiculoId = createDto.VeiculoId,
            Observacoes = createDto.Observacoes,
            DataAbertura = DateTime.UtcNow,
            StatusOS = EnumStatusOS.Recebida
        };

        var id = await _repository.CriarAsync(os);
        var criada = await _repository.ObterPorIdComItensAsync(id);
        return MapToDto(criada!);
    }

    public async Task<OrdemServicoDto?> GetByIdAsync(Guid id)
    {
        var os = await _repository.ObterPorIdComItensAsync(id);
        return os == null ? null : MapToDto(os);
    }

    public async Task<IEnumerable<OrdemServicoResumoDto>> GetAllAsync()
    {
        var lista = await _repository.ListarTodosAsync();
        return lista.Select(MapToResumoDto);
    }

    public async Task<OrdemServicoItemDto> AddItemAsync(Guid ordemServicoId, CreateOrdemServicoItemDto itemDto)
    {
        var os = await _repository.ObterPorIdComItensAsync(ordemServicoId)
            ?? throw new KeyNotFoundException("Ordem de serviço não encontrada");

        if (!Enum.TryParse<TipoOSItem>(itemDto.Tipo, ignoreCase: true, out var tipo))
            throw new ArgumentException("Tipo inválido. Use: servico, peca ou insumo");

        var item = new OrdemServicoItem(
            ordemServicoId,
            tipo,
            itemDto.ReferenciaId,
            itemDto.Descricao,
            itemDto.Quantidade,
            itemDto.PrecoUnitario
        );

        var salvo = await _repository.AdicionarItemAsync(item);

        os.Itens.Add(salvo);
        os.RecalcularTotal();
        await _repository.AtualizarTotalAsync(ordemServicoId, os.Total);

        return MapToItemDto(salvo);
    }

    public async Task RemoveItemAsync(Guid ordemServicoId, Guid itemId)
    {
        var os = await _repository.ObterPorIdComItensAsync(ordemServicoId)
            ?? throw new KeyNotFoundException("Ordem de serviço não encontrada");

        if (!os.Itens.Any(i => i.Id == itemId))
            throw new KeyNotFoundException("Item não encontrado nesta ordem de serviço");

        await _repository.RemoverItemAsync(ordemServicoId, itemId);

        os.Itens = os.Itens.Where(i => i.Id != itemId).ToList();
        os.RecalcularTotal();
        await _repository.AtualizarTotalAsync(ordemServicoId, os.Total);
    }

    public async Task<double> GetTempoMedioExecucaoAsync()
    {
        return await _repository.GetTempoMedioExecucaoHorasAsync();
    }

    // ─── Mappers ────────────────────────────────────────────

    private static OrdemServicoDto MapToDto(OrdemServico os) => new()
    {
        Id = os.Id,
        ClienteId = os.ClienteId,
        ClienteNome = os.Cliente?.Nome ?? string.Empty,
        VeiculoId = os.VeiculoId,
        VeiculoDescricao = os.Veiculo != null
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} ({os.Veiculo.Placa})"
            : string.Empty,
        Status = os.StatusOS.ToString(),
        Observacoes = os.Observacoes,
        Total = os.Total,
        DataAbertura = os.DataAbertura,
        DataFechamento = os.DataFechamento,
        Itens = os.Itens.Select(MapToItemDto).ToList()
    };

    private static OrdemServicoResumoDto MapToResumoDto(OrdemServico os) => new()
    {
        Id = os.Id,
        ClienteNome = os.Cliente?.Nome ?? string.Empty,
        VeiculoDescricao = os.Veiculo != null
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} ({os.Veiculo.Placa})"
            : string.Empty,
        Status = os.StatusOS.ToString(),
        Total = os.Total,
        DataAbertura = os.DataAbertura,
        DataFechamento = os.DataFechamento
    };

    private static OrdemServicoItemDto MapToItemDto(OrdemServicoItem i) => new()
    {
        Id = i.Id,
        Tipo = i.Tipo.ToString().ToLower(),
        ReferenciaId = i.ReferenciaId,
        Descricao = i.Descricao,
        Quantidade = i.Quantidade,
        PrecoUnitario = i.PrecoUnitario,
        Subtotal = i.Subtotal
    };
}