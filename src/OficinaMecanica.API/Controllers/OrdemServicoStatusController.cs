using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.API.Filters;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.EntregarOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.ForcarStatusOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.IniciarDiagnostico;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.NotificarConclusao;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.ObterHistoricoOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.RejeitarOS;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/ordens-servico")]
[Produces("application/json")]
[Authorize]
public class OrdemServicoStatusController : ControllerBase
{
    private readonly IIniciarDiagnosticoUseCase _iniciar;
    private readonly IMarcarAguardandoAprovacaoUseCase _enviarParaAprovacao;
    private readonly IAprovarOSUseCase _aprovar;
    private readonly IRejeitarOSUseCase _rejeitar;
    private readonly INotificarConclusaoUseCase _notificar;
    private readonly IEntregarOSUseCase _entregar;
    private readonly IForcarStatusOSUseCase _forcar;
    private readonly IObterHistoricoOSUseCase _historico;

    public OrdemServicoStatusController(
        IIniciarDiagnosticoUseCase iniciar,
        IMarcarAguardandoAprovacaoUseCase enviarParaAprovacao,
        IAprovarOSUseCase aprovar,
        IRejeitarOSUseCase rejeitar,
        INotificarConclusaoUseCase notificar,
        IEntregarOSUseCase entregar,
        IForcarStatusOSUseCase forcar,
        IObterHistoricoOSUseCase historico)
    {
        _iniciar = iniciar;
        _enviarParaAprovacao = enviarParaAprovacao;
        _aprovar = aprovar;
        _rejeitar = rejeitar;
        _notificar = notificar;
        _entregar = entregar;
        _forcar = forcar;
        _historico = historico;
    }

    private string UsuarioAtual() =>
        User?.FindFirst("email")?.Value
        ?? User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        ?? "anonimo";

    /// <summary>
    /// Inicia o diagnóstico do veículo, avançando a OS de Recebida para EmDiagnostico
    /// </summary>
    [HttpPatch("{id:guid}/iniciar-diagnostico")]
    [Idempotent]
    [Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarDiagnostico(Guid id)
    {
        var result = await _iniciar.ExecutarAsync(new IniciarDiagnosticoRequest(id, UsuarioAtual()));
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Envia a OS para aprovação do cliente, avançando de EmDiagnostico para AguardandoAprovacao
    /// </summary>
    [HttpPatch("{id:guid}/enviar-para-aprovacao")]
    [Idempotent]
    [Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnviarParaAprovacao(Guid id)
    {
        var result = await _enviarParaAprovacao.ExecutarAsync(new MarcarAguardandoAprovacaoRequest(id, UsuarioAtual()));
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Aprova o orçamento da OS, avançando de AguardandoAprovacao para EmExecucao
    /// </summary>
    [HttpPatch("{id:guid}/aprovar")]
    [Idempotent]
    [Authorize(Roles = "Admin,Cliente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Aprovar(Guid id)
    {
        var result = await _aprovar.ExecutarAsync(new AprovarOSRequest(id, UsuarioAtual()));
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Rejeita o orçamento da OS, encerrando o fluxo com status Rejeitada
    /// </summary>
    /// <remarks>
    /// O `motivo` é obrigatório e fica registrado no histórico da OS para rastreabilidade.
    /// </remarks>
    [HttpPatch("{id:guid}/rejeitar")]
    [Idempotent]
    [Authorize(Roles = "Admin,Cliente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rejeitar(Guid id, [FromBody] RejeitarOSRequest dto)
    {
        var result = await _rejeitar.ExecutarAsync(
            new RejeitarOSUseCaseRequest(id, UsuarioAtual(), dto?.Motivo ?? string.Empty));
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Notifica a conclusão do serviço, avançando a OS de EmExecucao para Finalizada
    /// </summary>
    [HttpPatch("{id:guid}/notificar-conclusao")]
    [Idempotent]
    [Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> NotificarConclusao(Guid id)
    {
        var result = await _notificar.ExecutarAsync(new NotificarConclusaoRequest(id, UsuarioAtual()));
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Registra a entrega do veículo ao cliente, encerrando a OS com status Entregue
    /// </summary>
    [HttpPatch("{id:guid}/entregar")]
    [Idempotent]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Entregar(Guid id)
    {
        var result = await _entregar.ExecutarAsync(new EntregarOSUseCaseRequest(id, UsuarioAtual()));
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Força a OS para um status arbitrário, ignorando as regras de transição do fluxo normal
    /// </summary>
    /// <remarks>
    /// Exclusivo para Admin. Use para corrigir estados inválidos ou avançar manualmente etapas sem endpoint dedicado
    /// (ex.: `EmDiagnostico → AguardandoAprovacao` com `novoStatus: 3`).
    /// Todas as vezes o uso é registrado no histórico com o motivo informado.
    /// </remarks>
    [HttpPatch("{id:guid}/status")]
    [Idempotent]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForcarStatus(Guid id, [FromBody] TransicaoStatusOSRequest dto)
    {
        var result = await _forcar.ExecutarAsync(
            new ForcarStatusOSRequest(id, dto.NovoStatus, UsuarioAtual(), dto?.Motivo ?? string.Empty));
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Retorna o histórico completo de transições de status de uma OS
    /// </summary>
    [HttpGet("{id:guid}/historico")]
    [Authorize(Roles = "Admin,Mecanico,Cliente")]
    [ProducesResponseType(typeof(IEnumerable<HistoricoStatusOSResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Historico(Guid id)
    {
        var result = await _historico.ExecutarAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }
}
