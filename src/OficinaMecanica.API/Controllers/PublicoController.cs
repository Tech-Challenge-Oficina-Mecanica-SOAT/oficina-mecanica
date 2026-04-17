using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class PublicoController : ControllerBase
{
    private readonly IOrdemServicoRepository _osRepository;

    public PublicoController(IOrdemServicoRepository osRepository) =>
        _osRepository = osRepository;

    [HttpGet("os/{id:guid}/status")]
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
