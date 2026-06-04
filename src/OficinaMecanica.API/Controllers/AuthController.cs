using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.UseCases.Auth.AutenticarUsuario;
using OficinaMecanica.Application.UseCases.Auth.RegistrarUsuario;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAutenticarUsuarioUseCase _autenticar;
    private readonly IRegistrarUsuarioUseCase _registrar;

    public AuthController(IAutenticarUsuarioUseCase autenticar, IRegistrarUsuarioUseCase registrar)
    {
        _autenticar = autenticar;
        _registrar = registrar;
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT
    /// </summary>
    /// <remarks>
    /// O token retornado deve ser enviado no header `Authorization: Bearer {token}` em todas as rotas protegidas.
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _autenticar.ExecutarAsync(request);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Registra um novo usuário no sistema
    /// </summary>
    /// <remarks>
    /// Perfis disponíveis: `0 = Admin`, `1 = Mecanico`, `2 = Cliente`.
    /// Registro anônimo cria apenas perfil Cliente. Para Admin ou Mecanico é necessário autenticação de Admin.
    /// O e-mail deve ser único — tentativas de duplicata retornam `409 Conflict`.
    /// </remarks>
    [HttpPost("registrar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest request)
    {
        if (request.Perfil != Domain.Enums.Perfil.Cliente && !User.Identity!.IsAuthenticated)
            return Unauthorized(new { mensagem = "Apenas usuários autenticados podem registrar outros perfis." });

        if (request.Perfil != Domain.Enums.Perfil.Cliente && !User.IsInRole("Admin"))
            return Forbid();

        var result = await _registrar.ExecutarAsync(request);
        return result.IsSuccess
            ? Created(string.Empty, new { id = result.Value })
            : this.MapError(result);
    }
}
