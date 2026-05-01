using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;

    public VeiculosController(IVeiculoService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    /// <summary>
    /// Lista todos os veículos cadastrados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VeiculoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var veiculos = await _veiculoService.GetAllAsync();
        return Ok(veiculos);
    }

    /// <summary>
    /// Obtém um veículo por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var veiculo = await _veiculoService.GetByIdAsync(id);
        if (veiculo == null)
            return NotFound();
        return Ok(veiculo);
    }

    /// <summary>
    /// Busca um veículo pela placa
    /// </summary>
    /// <remarks>
    /// A placa deve estar no formato Mercosul (ex.: `ABC1D23`) ou no formato antigo (ex.: `ABC1234`).
    /// </remarks>
    [HttpGet("placa/{placa}")]
    [ProducesResponseType(typeof(VeiculoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPlaca(string placa)
    {
        var veiculo = await _veiculoService.GetByPlacaAsync(placa);
        if (veiculo == null)
            return NotFound();
        return Ok(veiculo);
    }

    /// <summary>
    /// Lista todos os veículos de um cliente
    /// </summary>
    [HttpGet("cliente/{clienteId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<VeiculoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClienteId(Guid clienteId)
    {
        var veiculos = await _veiculoService.GetByClienteIdAsync(clienteId);
        return Ok(veiculos);
    }

    /// <summary>
    /// Cadastra um novo veículo associado a um cliente existente
    /// </summary>
    /// <remarks>
    /// O `clienteId` deve corresponder a um cliente ativo. A placa deve ser única no sistema.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(VeiculoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateVeiculoDto createDto)
    {
        try
        {
            var veiculo = await _veiculoService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = veiculo.Id }, veiculo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
    /// Atualiza os dados de um veículo existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVeiculoDto updateDto)
    {
        try
        {
            var veiculo = await _veiculoService.UpdateAsync(id, updateDto);
            return Ok(veiculo);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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
    /// Remove permanentemente um veículo
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _veiculoService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
