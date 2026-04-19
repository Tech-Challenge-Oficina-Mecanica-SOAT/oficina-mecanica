using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServicosController : ControllerBase
{
    private readonly IServicoService _servicoService;

    public ServicosController(IServicoService servicoService)
    {
        _servicoService = servicoService;
    }

    /// <summary>
    /// Obtém todos os serviços
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServicoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var servicos = await _servicoService.GetAllAsync();
        return Ok(servicos);
    }

    /// <summary>
    /// Obtém apenas serviços ativos
    /// </summary>
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(IEnumerable<ServicoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAtivos()
    {
        var servicos = await _servicoService.GetAtivosAsync();
        return Ok(servicos);
    }

    /// <summary>
    /// Obtém um serviço por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var servico = await _servicoService.GetByIdAsync(id);
        if (servico == null)
            return NotFound();
        return Ok(servico);
    }

    /// <summary>
    /// Obtém um serviço por nome
    /// </summary>
    [HttpGet("nome/{nome}")]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNome(string nome)
    {
        var servicos = await _servicoService.GetByNomeAsync(nome);
        var lista = servicos as IEnumerable<ServicoDto>;
        
        if (lista == null || !lista.Any())
            return NotFound();
            
        return Ok(lista);
    }

    /// <summary>
    /// Cria um novo serviço
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServicoDto createDto)
    {
        try
        {
            var servico = await _servicoService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = servico.Id }, servico);
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
    /// Atualiza um serviço existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServicoDto updateDto)
    {
        try
        {
            var servico = await _servicoService.UpdateAsync(id, updateDto);
            return Ok(servico);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Serviço não encontrado" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove um serviço
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _servicoService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Serviço não encontrado" });
        }
    }

    /// <summary>
    /// Ativa um serviço
    /// </summary>
    [HttpPatch("{id:guid}/ativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(Guid id)
    {
        try
        {
            await _servicoService.AtivarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Serviço não encontrado" });
        }
    }

    /// <summary>
    /// Desativa um serviço
    /// </summary>
    [HttpPatch("{id:guid}/desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id)
    {
        try
        {
            await _servicoService.DesativarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Serviço não encontrado" });
        }
    }
}
