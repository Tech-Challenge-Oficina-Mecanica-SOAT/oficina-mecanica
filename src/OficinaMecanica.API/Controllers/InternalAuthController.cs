using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.UseCases.Auth.AutenticarPorCpf;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("internal/auth")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public class InternalAuthController : ControllerBase
{
    private readonly IAutenticarPorCpfUseCase _autenticarPorCpf;

    public InternalAuthController(IAutenticarPorCpfUseCase autenticarPorCpf) =>
        _autenticarPorCpf = autenticarPorCpf;

    /// <summary>
    /// Autentica um cliente via CPF e retorna um token JWT
    /// </summary>
    /// <remarks>
    /// Endpoint interno destinado à Lambda de autenticação. Requer API Key válida no header `X-Internal-Api-Key`.
    /// </remarks>
    [HttpPost("cpf-verify")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CpfVerify([FromBody] AutenticarPorCpfRequest request)
    {
        var result = await _autenticarPorCpf.ExecutarAsync(request);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }
}
