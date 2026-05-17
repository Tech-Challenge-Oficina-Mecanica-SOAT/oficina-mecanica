using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;
using OficinaMecanica.Application.UseCases.OrdemServico.AdicionarItensOS;
using OficinaMecanica.Application.UseCases.OrdemServico.ConsultarOrdemServico;
using OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;
using OficinaMecanica.Application.UseCases.OrdemServico.ObterTempoMedioExecucao;
using OficinaMecanica.Application.UseCases.OrdemServico.RemoverItemOS;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/ordens-servico")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class OrdemServicosController : ControllerBase
{
    private readonly IAbrirOrdemServicoUseCase _abrir;
    private readonly IConsultarOrdemServicoUseCase _consultar;
    private readonly IListarOrdensServicoUseCase _listar;
    private readonly IAdicionarItensOSUseCase _adicionarItens;
    private readonly IRemoverItemOSUseCase _removerItem;
    private readonly IObterTempoMedioExecucaoUseCase _tempoMedio;

    public OrdemServicosController(
        IAbrirOrdemServicoUseCase abrir,
        IConsultarOrdemServicoUseCase consultar,
        IListarOrdensServicoUseCase listar,
        IAdicionarItensOSUseCase adicionarItens,
        IRemoverItemOSUseCase removerItem,
        IObterTempoMedioExecucaoUseCase tempoMedio)
    {
        _abrir = abrir;
        _consultar = consultar;
        _listar = listar;
        _adicionarItens = adicionarItens;
        _removerItem = removerItem;
        _tempoMedio = tempoMedio;
    }

    /// <summary>
    /// Lista todas as ordens de serviço
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoResumoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listar.ExecutarAsync(Unit.Value);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Obtém uma ordem de serviço por ID com todos os itens
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _consultar.ExecutarAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Cria uma nova ordem de serviço vinculando cliente e veículo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AbrirOrdemServicoRequest request)
    {
        var result = await _abrir.ExecutarAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : this.MapError(result);
    }

    /// <summary>
    /// Adiciona os itens (serviço, peça ou insumo) à ordem de serviço.
    /// O total é calculado automaticamente.
    /// </summary>
    /// <remarks>
    /// O campo **tipo** aceita: `servico`, `peca` ou `insumo`
    /// </remarks>
    [HttpPost("{id:guid}/itens")]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] List<AdicionarOSItemRequest> itens)
    {
        var request = new AdicionarItensOSRequest { OrdemServicoId = id, Itens = itens };
        var result = await _adicionarItens.ExecutarAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : this.MapError(result);
    }

    /// <summary>
    /// Remove um item da ordem de serviço.
    /// O total é recalculado automaticamente.
    /// </summary>
    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        var result = await _removerItem.ExecutarAsync(new RemoverItemOSRequest(id, itemId));
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Retorna o tempo médio de execução das ordens de serviço finalizadas (em horas)
    /// </summary>
    [HttpGet("tempo-medio-execucao")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTempoMedioExecucao()
    {
        var result = await _tempoMedio.ExecutarAsync(Unit.Value);
        return result.IsSuccess
            ? Ok(new { tempoMedioHoras = result.Value })
            : this.MapError(result);
    }
}
