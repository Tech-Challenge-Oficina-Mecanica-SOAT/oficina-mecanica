using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[AllowAnonymous]
public class PublicoController : ControllerBase
{
    private readonly IOrdemServicoRepository _osRepository;

    public PublicoController(IOrdemServicoRepository osRepository) =>
        _osRepository = osRepository;

    /// <summary>
    /// Consulta o status atual de uma OS sem necessidade de autenticação
    /// </summary>
    /// <remarks>
    /// Endpoint público destinado ao cliente final. Retorna apenas `osId`, `status` e `atualizadoEm` —
    /// nenhum dado pessoal ou financeiro é exposto. Ideal para integração com portais e apps de rastreamento.
    /// </remarks>
    [HttpGet("os/{id:guid}/status")]
    [ProducesResponseType(typeof(PainelStatusOSDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusOS(Guid id)
    {
        var os = await _osRepository.ObterPorIdAsync(id);
        if (os is null)
            return NotFound(new { mensagem = $"Ordem de serviço {id} não encontrada." });

        var dto = new PainelStatusOSDto(
            OsId: os.Id,
            Status: os.StatusOS.ToString(),
            AtualizadoEm: os.DataFechamento ?? os.DataAbertura
        );

        return Ok(dto);
    }
}
