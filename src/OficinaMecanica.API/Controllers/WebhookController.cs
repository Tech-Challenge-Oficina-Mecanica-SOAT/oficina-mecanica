using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOrcamentoPorEmail;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class WebhookController : ControllerBase
{
    private readonly IAprovarOrcamentoPorEmailUseCase _useCase;

    public WebhookController(IAprovarOrcamentoPorEmailUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet("ordens-servico/aprovar/{token}")]
    public async Task<IActionResult> AprovarOrcamento(string token, [FromQuery] bool aprovado)
    {
        var result = await _useCase.ExecutarAsync(new AprovarOrcamentoPorEmailRequest(token, aprovado));
        
        if (!result.IsSuccess)
        {
            return Content($@"
                <html>
                <body style='font-family: Arial; text-align: center; margin-top: 50px;'>
                    <h1 style='color: red;'>❌ Erro</h1>
                    <p>{result.Error}</p>
                </body>
                </html>", "text/html");
        }
        
        if (result.Value == "aprovado")
        {
            return Content(@"
                <html>
                <body style='font-family: Arial; text-align: center; margin-top: 50px;'>
                    <h1 style='color: green;'>✅ Orçamento Aprovado!</h1>
                    <p>Sua ordem de serviço entrou em execução.</p>
                    <p>Obrigado pela confiança!</p>
                </body>
                </html>", "text/html");
        }
        
        return Content(@"
            <html>
            <body style='font-family: Arial; text-align: center; margin-top: 50px;'>
                <h1 style='color: red;'>❌ Orçamento Recusado</h1>
                <p>Sua ordem de serviço foi cancelada.</p>
                <p>Entre em contato para negociar um novo orçamento.</p>
            </body>
            </html>", "text/html");
    }
}