using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/testes")]
[Authorize(Roles = "Admin")]
public class TestesController : ControllerBase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public TestesController(IOrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    private string GerarTokenUnico()
    {
        // Gera token de 32 caracteres usando dois GUIDs
        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        return token.Substring(0, 32);
    }

    /// <summary>
    /// Gera um token único para testar a aprovação de orçamento
    /// </summary>
    /// <param name="osId">ID da Ordem de Serviço</param>
    [HttpPost("gerar-token-aprovacao/{osId:guid}")]
    public async Task<IActionResult> GerarTokenAprovacao(Guid osId)
    {
        var os = await _ordemServicoRepository.ObterPorIdAsync(osId);
        if (os == null)
            return NotFound(new { erro = "OS não encontrada" });

        // Verifica se já tem token não usado
        if (!string.IsNullOrEmpty(os.TokenAprovacao) && !os.TokenUsado)
        {
            return Ok(new { 
                osId = os.Id,
                token = os.TokenAprovacao,
                mensagem = "Token já existente (não foi usado)",
                url = $"/api/webhooks/ordens-servico/aprovar/{os.TokenAprovacao}"
            });
        }

        // Gera token único de 32 caracteres
        var token = GerarTokenUnico();

        os.TokenAprovacao = token;
        os.TokenUsado = false;
        await _ordemServicoRepository.UpdateAsync(os);

        return Ok(new { 
            osId = os.Id,
            token,
            url = $"/api/webhooks/ordens-servico/aprovar/{token}",
            instrucao = "Use este token no endpoint POST /api/webhooks/ordens-servico/aprovar/{token} com body: {\"aprovado\": true/false}"
        });
    }

    /// <summary>
    /// Consulta informações do token de uma OS
    /// </summary>
    [HttpGet("consultar-token/{osId:guid}")]
    public async Task<IActionResult> ConsultarToken(Guid osId)
    {
        var os = await _ordemServicoRepository.ObterPorIdAsync(osId);
        if (os == null)
            return NotFound(new { erro = "OS não encontrada" });

        return Ok(new
        {
            osId = os.Id,
            status = os.StatusOS.ToString(),
            tokenAprovacao = os.TokenAprovacao,
            tokenUsado = os.TokenUsado
        });
    }
}