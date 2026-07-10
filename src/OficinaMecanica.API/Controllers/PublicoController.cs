using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
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
    [HttpGet("os/{id:guid}/status")]
    [ProducesResponseType(typeof(PainelStatusOSResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusOS(Guid id)
    {
        var os = await _osRepository.ObterPorIdAsync(id);
        if (os is null)
            return NotFound(new { mensagem = $"Ordem de serviço {id} não encontrada." });

        var dto = new PainelStatusOSResponse(
            OsId: os.Id,
            Status: os.StatusOS.ToString(),
            AtualizadoEm: os.DataFechamento ?? os.DataAbertura
        );

        return Ok(dto);
    }

    /// <summary>
    /// Página pública de acompanhamento de status de uma OS
    /// </summary>
    /// <remarks>
    /// Endpoint destinado ao link enviado por e-mail ao cliente na abertura da OS.
    /// Renderiza uma página HTML com status atual, dados do veículo e histórico
    /// de atualizações. Não requer autenticação.
    /// </remarks>
    [HttpGet("os/{id:guid}")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPainelStatusOS(Guid id)
    {
        var os = await _osRepository.ObterPorIdComHistoricoAsync(id);
        if (os is null)
            return NotFound("Ordem de serviço não encontrada.");

        var html = PainelStatusHtmlBuilder.Construir(os);
        return Content(html, "text/html; charset=utf-8");
    }
}