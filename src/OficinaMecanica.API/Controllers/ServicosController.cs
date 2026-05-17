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
public class ServicosController : ControllerBase
{
    private readonly IServicoService _servicoService;

    public ServicosController(IServicoService servicoService)
    {
        _servicoService = servicoService;
    }

    /// <summary>
    /// Lista todos os serviços do catálogo (ativos e inativos)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServicoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var servicos = await _servicoService.GetAllAsync();
        return Ok(servicos);
    }

    /// <summary>
    /// Lista apenas os serviços ativos disponíveis para inclusão em ordens de serviço
    /// </summary>
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(IEnumerable<ServicoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAtivos()
    {
        var servicos = await _servicoService.GetAtivosAsync();
        return Ok(servicos);
    }

    /// <summary>
    /// Obtém um serviço por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var servico = await _servicoService.GetByIdAsync(id);
        if (servico == null)
            return NotFound();
        return Ok(servico);
    }

    /// <summary>
    /// Busca serviços pelo nome (busca parcial)
    /// </summary>
    [HttpGet("nome/{nome}")]
    [ProducesResponseType(typeof(IEnumerable<ServicoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNome(string nome)
    {
        var servicos = await _servicoService.GetByNomeAsync(nome);
        var lista = servicos;

        if (lista == null || !lista.Any())
            return NotFound();

        return Ok(lista);
    }

    /// <summary>
    /// Cadastra um novo serviço no catálogo da oficina
    /// </summary>
    /// <remarks>
    /// O `valor` deve ser maior que zero. O serviço é criado como ativo por padrão e fica
    /// disponível imediatamente para ser adicionado a ordens de serviço.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CriarServicoRequest createDto)
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
    /// Atualiza os dados de um serviço existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarServicoRequest updateDto)
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
    /// Remove permanentemente um serviço do catálogo
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
    /// Reativa um serviço previamente desativado, tornando-o disponível para novas OS
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
    /// Desativa um serviço sem removê-lo, impedindo sua inclusão em novas ordens de serviço
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
