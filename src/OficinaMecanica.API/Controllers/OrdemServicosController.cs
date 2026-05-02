using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/ordens-servico")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class OrdemServicosController : ControllerBase
{
    private readonly IOrdemServicoService _service;

    public OrdemServicosController(IOrdemServicoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas as ordens de serviço
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoResumoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _service.GetAllAsync();
        return Ok(lista);
    }

    /// <summary>
    /// Obtém uma ordem de serviço por ID com todos os itens
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var os = await _service.GetByIdAsync(id);
        if (os == null)
            return NotFound(new { message = "Ordem de serviço não encontrada" });
        return Ok(os);
    }

    /// <summary>
    /// Cria uma nova ordem de serviço vinculando cliente e veículo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrdemServicoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrdemServicoDto createDto)
    {
        try
        {
            var os = await _service.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = os.Id }, os);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Adiciona os itens (serviço, peça ou insumo) à ordem de serviço.
    /// O total é calculado automaticamente.
    /// </summary>
    /// <remarks>
    /// O campo **tipo** aceita: `servico`, `peca` ou `insumo`
    /// </remarks>
    [HttpPost("{id:guid}/itens")]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] List<CreateOrdemServicoItemDto> itensDto)
    {
        try
        {
            var itens = await _service.AddItensAsync(id, itensDto);
            return CreatedAtAction(nameof(GetById), new { id }, itens);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
        try
        {
            await _service.RemoveItemAsync(id, itemId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retorna o tempo médio de execução das ordens de serviço finalizadas (em horas)
    /// </summary>
    [HttpGet("tempo-medio-execucao")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTempoMedioExecucao()
    {
        var horas = await _service.GetTempoMedioExecucaoAsync();
        return Ok(new { tempoMedioHoras = horas });
    }
}