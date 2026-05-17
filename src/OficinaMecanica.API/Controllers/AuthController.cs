using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthController(IUsuarioService usuarioService, ITokenGenerator tokenGenerator)
    {
        _usuarioService = usuarioService;
        _tokenGenerator = tokenGenerator;
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT
    /// </summary>
    /// <remarks>
    /// O token retornado deve ser enviado no header `Authorization: Bearer {token}` em todas as rotas protegidas.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        var usuario = await _usuarioService.AutenticarAsync(dto.Email, dto.Senha);
        if (usuario is null)
            return Unauthorized(new { mensagem = "Credenciais inválidas." });

        var token = _tokenGenerator.GerarParaUsuario(usuario);
        return Ok(token);
    }

    /// <summary>
    /// Registra um novo usuário no sistema
    /// </summary>
    /// <remarks>
    /// Perfis disponíveis: `0 = Admin`, `1 = Mecanico`, `2 = Cliente`.
    /// O e-mail deve ser único — tentativas de duplicata retornam `409 Conflict`.
    /// </remarks>
    [HttpPost("registrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest dto)
    {
        try
        {
            var usuario = await _usuarioService.RegistrarAsync(dto);
            return Created(string.Empty, new { usuario.Id, usuario.Email, usuario.Perfil });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }
}
