using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/ordens-servico")]
[Authorize]
public class OrdemServicoStatusController : ControllerBase
{
    private readonly IOrdemServicoStatusService _service;

    public OrdemServicoStatusController(IOrdemServicoStatusService service) =>
        _service = service;

    private string UsuarioAtual() =>
        User?.FindFirst("email")?.Value
        ?? User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        ?? "anonimo";

    [HttpPatch("{id:guid}/iniciar-diagnostico")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> IniciarDiagnostico(Guid id) =>
        await TryRunAsync(() => _service.IniciarDiagnosticoAsync(id, UsuarioAtual()));

    [HttpPatch("{id:guid}/aprovar")]
    [Authorize(Roles = "Admin,Cliente")]
    public async Task<IActionResult> Aprovar(Guid id) =>
        await TryRunAsync(() => _service.AprovarAsync(id, UsuarioAtual()));

    [HttpPatch("{id:guid}/rejeitar")]
    [Authorize(Roles = "Admin,Cliente")]
    public async Task<IActionResult> Rejeitar(Guid id, [FromBody] RejeitarOSDto dto) =>
        await TryRunAsync(() => _service.RejeitarAsync(id, UsuarioAtual(), dto?.Motivo ?? string.Empty));

    [HttpPatch("{id:guid}/notificar-conclusao")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> NotificarConclusao(Guid id) =>
        await TryRunAsync(() => _service.NotificarConclusaoAsync(id, UsuarioAtual()));

    [HttpPatch("{id:guid}/entregar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Entregar(Guid id) =>
        await TryRunAsync(() => _service.EntregarAsync(id, UsuarioAtual()));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ForcarStatus(Guid id, [FromBody] TransicaoStatusOSDto dto) =>
        await TryRunAsync(() => _service.ForcarStatusAsync(id, dto.NovoStatus, UsuarioAtual(), dto?.Motivo ?? string.Empty));

    [HttpGet("{id:guid}/historico")]
    [Authorize(Roles = "Admin,Mecanico,Cliente")]
    public async Task<IActionResult> Historico(Guid id)
    {
        try
        {
            var historico = await _service.ObterHistoricoAsync(id);
            return Ok(historico);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<IActionResult> TryRunAsync(Func<Task> action)
    {
        try
        {
            await action();
            return NoContent();
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
}
