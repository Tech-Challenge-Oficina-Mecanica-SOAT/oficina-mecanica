using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IJwtService _jwtService;

    public AuthController(IUsuarioService usuarioService, IJwtService jwtService)
    {
        _usuarioService = usuarioService;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await _usuarioService.AutenticarAsync(dto.Email, dto.Senha);
        if (usuario is null)
            return Unauthorized(new { mensagem = "Credenciais inválidas." });

        var token = _jwtService.GerarToken(usuario);
        return Ok(token);
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDto dto)
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
