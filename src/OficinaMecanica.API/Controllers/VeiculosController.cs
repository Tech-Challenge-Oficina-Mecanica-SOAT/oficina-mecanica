using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;

    public VeiculosController(IVeiculoService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var veiculos = await _veiculoService.GetAllAsync();
        return Ok(veiculos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var veiculo = await _veiculoService.GetByIdAsync(id);
        if (veiculo == null)
            return NotFound();
        return Ok(veiculo);
    }

    [HttpGet("placa/{placa}")]
    public async Task<IActionResult> GetByPlaca(string placa)
    {
        var veiculo = await _veiculoService.GetByPlacaAsync(placa);
        if (veiculo == null)
            return NotFound();
        return Ok(veiculo);
    }

    [HttpGet("cliente/{clienteId:guid}")]
    public async Task<IActionResult> GetByClienteId(Guid clienteId)
    {
        var veiculos = await _veiculoService.GetByClienteIdAsync(clienteId);
        return Ok(veiculos);
    }

    [HttpPost]
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

    [HttpPut("{id:guid}")]
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

    [HttpDelete("{id:guid}")]
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
