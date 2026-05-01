using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/ordens-servico")]
[Produces("application/json")]
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

    /// <summary>
    /// Inicia o diagnóstico do veículo, avançando a OS de Recebida para EmDiagnostico
    /// </summary>
    [HttpPatch("{id:guid}/iniciar-diagnostico")]
    [Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarDiagnostico(Guid id) =>
        await TryRunAsync(() => _service.IniciarDiagnosticoAsync(id, UsuarioAtual()));

    /// <summary>
    /// Aprova o orçamento da OS, avançando de AguardandoAprovacao para EmExecucao
    /// </summary>
    [HttpPatch("{id:guid}/aprovar")]
    [Authorize(Roles = "Admin,Cliente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Aprovar(Guid id) =>
        await TryRunAsync(() => _service.AprovarAsync(id, UsuarioAtual()));

    /// <summary>
    /// Rejeita o orçamento da OS, encerrando o fluxo com status Rejeitada
    /// </summary>
    /// <remarks>
    /// O `motivo` é obrigatório e fica registrado no histórico da OS para rastreabilidade.
    /// </remarks>
    [HttpPatch("{id:guid}/rejeitar")]
    [Authorize(Roles = "Admin,Cliente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rejeitar(Guid id, [FromBody] RejeitarOSDto dto) =>
        await TryRunAsync(() => _service.RejeitarAsync(id, UsuarioAtual(), dto?.Motivo ?? string.Empty));

    /// <summary>
    /// Notifica a conclusão do serviço, avançando a OS de EmExecucao para Finalizada
    /// </summary>
    [HttpPatch("{id:guid}/notificar-conclusao")]
    [Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> NotificarConclusao(Guid id) =>
        await TryRunAsync(() => _service.NotificarConclusaoAsync(id, UsuarioAtual()));

    /// <summary>
    /// Registra a entrega do veículo ao cliente, encerrando a OS com status Entregue
    /// </summary>
    [HttpPatch("{id:guid}/entregar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Entregar(Guid id) =>
        await TryRunAsync(() => _service.EntregarAsync(id, UsuarioAtual()));

    /// <summary>
    /// Força a OS para um status arbitrário, ignorando as regras de transição do fluxo normal
    /// </summary>
    /// <remarks>
    /// Exclusivo para Admin. Use para corrigir estados inválidos ou avançar manualmente etapas sem endpoint dedicado
    /// (ex.: `EmDiagnostico → AguardandoAprovacao` com `novoStatus: 3`).
    /// Todo uso é registrado no histórico com o motivo informado.
    /// </remarks>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForcarStatus(Guid id, [FromBody] TransicaoStatusOSDto dto) =>
        await TryRunAsync(() => _service.ForcarStatusAsync(id, dto.NovoStatus, UsuarioAtual(), dto?.Motivo ?? string.Empty));

    /// <summary>
    /// Retorna o histórico completo de transições de status de uma OS
    /// </summary>
    [HttpGet("{id:guid}/historico")]
    [Authorize(Roles = "Admin,Mecanico,Cliente")]
    [ProducesResponseType(typeof(IEnumerable<HistoricoStatusOSDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
