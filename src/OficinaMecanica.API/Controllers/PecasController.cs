using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class PecasController : ControllerBase
{
    private readonly IPecaService _pecaService;

    public PecasController(IPecaService pecaService)
    {
        _pecaService = pecaService;
    }

    /// <summary>
    /// Lista todas as peças e insumos do estoque
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var pecas = await _pecaService.GetAllAsync();
        return Ok(pecas);
    }

    /// <summary>
    /// Obtém uma peça por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var peca = await _pecaService.GetByIdAsync(id);
        if (peca == null)
            return NotFound(new { message = $"Peça com ID {id} não encontrada" });

        return Ok(peca);
    }

    /// <summary>
    /// Busca uma peça pelo código interno (SKU)
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCodigo(string codigo)
    {
        var todasPecas = await _pecaService.GetAllAsync();
        var peca = todasPecas.FirstOrDefault(p => p.Codigo == codigo);

        if (peca == null)
            return NotFound(new { message = $"Peça com código '{codigo}' não encontrada" });

        return Ok(peca);
    }

    /// <summary>
    /// Lista peças com estoque igual ou abaixo do limite informado
    /// </summary>
    /// <remarks>
    /// Use este endpoint para identificar itens que precisam de reposição.
    /// O parâmetro `limite` tem valor padrão `10` quando não informado.
    /// </remarks>
    [HttpGet("estoque-baixo")]
    [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstoqueBaixo([FromQuery] int limite = 10)
    {
        var pecas = await _pecaService.GetByEstoqueBaixoAsync(limite);
        return Ok(pecas);
    }

    /// <summary>
    /// Retorna a quantidade atual em estoque de uma peça
    /// </summary>
    [HttpGet("{id:guid}/estoque")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEstoque(Guid id)
    {
        var existe = await _pecaService.GetByIdAsync(id);
        if (existe == null)
            return NotFound(new { message = $"Peça com ID {id} não encontrada" });

        var estoque = await _pecaService.GetEstoqueAsync(id);
        return Ok(new { pecaId = id, estoque });
    }

    /// <summary>
    /// Cadastra uma nova peça ou insumo no catálogo
    /// </summary>
    /// <remarks>
    /// O `codigo` deve ser único. O estoque inicial pode ser zero; use `PATCH /{id}/estoque` para movimentar.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CriarPecaRequest createDto)
    {
        try
        {
            var peca = await _pecaService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = peca.Id }, peca);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza os dados cadastrais de uma peça (nome, código, preço)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarPecaRequest updateDto)
    {
        try
        {
            var peca = await _pecaService.UpdateAsync(id, updateDto);
            if (peca == null)
                return NotFound(new { message = $"Peça com ID {id} não encontrada" });

            return Ok(peca);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Movimenta o estoque de uma peça (entrada ou saída)
    /// </summary>
    /// <remarks>
    /// O campo **tipoOperacao** aceita: `incrementar` (entrada) ou `decrementar` (saída/uso em OS).
    /// Decrementar além do saldo disponível retorna `400 Bad Request`.
    /// </remarks>
    [HttpPatch("{id:guid}/estoque")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEstoque(Guid id, [FromBody] AtualizarEstoqueRequest updateEstoqueDto)
    {
        try
        {
            var existe = await _pecaService.GetByIdAsync(id);
            if (existe == null)
                return NotFound(new { message = $"Peça com ID {id} não encontrada" });

            await _pecaService.UpdateEstoqueAsync(id, updateEstoqueDto);
            var novoEstoque = await _pecaService.GetEstoqueAsync(id);

            return Ok(new
            {
                success = true,
                message = $"Estoque {(updateEstoqueDto.TipoOperacao == "incrementar" ? "incrementado" : "decrementado")} com sucesso",
                novoEstoque
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove permanentemente uma peça do catálogo
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existe = await _pecaService.GetByIdAsync(id);
        if (existe == null)
            return NotFound(new { message = $"Peça com ID {id} não encontrada" });

        await _pecaService.DeleteAsync(id);
        return NoContent();
    }
}
