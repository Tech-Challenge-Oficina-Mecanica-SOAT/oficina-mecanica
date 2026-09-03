using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Filters;
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
    [Idempotent(ChaveDeArgumento = "token")]
    public async Task<IActionResult> AprovarOrcamento(string token, [FromQuery] bool aprovado)
    {
        var result = await _useCase.ExecutarAsync(new AprovarOrcamentoPorEmailRequest(token, aprovado));
        
        if (!result.IsSuccess)
        {
            return Content($@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Erro - Oficina Mecânica</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }}
        .container {{
            max-width: 500px;
            width: 100%;
            background: white;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.2);
            overflow: hidden;
            animation: fadeIn 0.5s ease-in-out;
        }}
        @keyframes fadeIn {{
            from {{ opacity: 0; transform: translateY(-20px); }}
            to {{ opacity: 1; transform: translateY(0); }}
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            color: white;
            font-size: 28px;
            margin-bottom: 10px;
        }}
        .icon {{
            font-size: 64px;
            margin-bottom: 10px;
            color: white;
        }}
        .content {{
            padding: 40px 30px;
            text-align: center;
        }}
        .content p {{
            color: #555;
            line-height: 1.6;
            margin-bottom: 20px;
        }}
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 25px;
            font-weight: 600;
            margin-top: 20px;
            transition: transform 0.3s, box-shadow 0.3s;
        }}
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(0,0,0,0.2);
        }}
        .footer {{
            background: #f8f9fa;
            padding: 15px;
            text-align: center;
            font-size: 12px;
            color: #999;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='icon'>!</div>
            <h1>Erro</h1>
        </div>
        <div class='content'>
            <p>{result.Error}</p>
        </div>
        <div class='footer'>
            <p>Oficina Mecânica - Atendimento de qualidade</p>
        </div>
    </div>
</body>
</html>", "text/html; charset=utf-8");
        }
        
        if (result.Value == "aprovado")
        {
            return Content(@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Orçamento Aprovado - Oficina Mecânica</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }
        .container {
            max-width: 500px;
            width: 100%;
            background: white;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.2);
            overflow: hidden;
            animation: fadeIn 0.5s ease-in-out;
        }
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(-20px); }
            to { opacity: 1; transform: translateY(0); }
        }
        .header {
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            padding: 40px 30px;
            text-align: center;
        }
        .header h1 {
            color: white;
            font-size: 28px;
            margin-bottom: 10px;
        }
        .icon {
            font-size: 80px;
            margin-bottom: 10px;
            color: white;
        }
        .content {
            padding: 40px 30px;
            text-align: center;
        }
        .content p {
            color: #555;
            line-height: 1.6;
            margin-bottom: 15px;
        }
        .button {
            display: inline-block;
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 25px;
            font-weight: 600;
            margin-top: 20px;
            transition: transform 0.3s, box-shadow 0.3s;
            border: none;
            cursor: pointer;
        }
        .button:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(0,0,0,0.2);
        }
        .footer {
            background: #f8f9fa;
            padding: 15px;
            text-align: center;
            font-size: 12px;
            color: #999;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='icon'>✓</div>
            <h1>Orçamento Aprovado</h1>
        </div>
        <div class='content'>
            <p>Sua ordem de serviço entrou em execução.</p>
            <p>Obrigado pela confiança!</p>
        </div>
        <div class='footer'>
            <p>Oficina Mecânica - Atendimento de qualidade</p>
        </div>
    </div>
</body>
</html>", "text/html; charset=utf-8");
        }
        
        return Content(@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Orçamento Recusado - Oficina Mecânica</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }
        .container {
            max-width: 500px;
            width: 100%;
            background: white;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.2);
            overflow: hidden;
            animation: fadeIn 0.5s ease-in-out;
        }
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(-20px); }
            to { opacity: 1; transform: translateY(0); }
        }
        .header {
            background: linear-gradient(135deg, #eb3349 0%, #f45c43 100%);
            padding: 40px 30px;
            text-align: center;
        }
        .header h1 {
            color: white;
            font-size: 28px;
            margin-bottom: 10px;
        }
        .icon {
            font-size: 80px;
            margin-bottom: 10px;
            color: white;
        }
        .content {
            padding: 40px 30px;
            text-align: center;
        }
        .content p {
            color: #555;
            line-height: 1.6;
            margin-bottom: 15px;
        }
        .button {
            display: inline-block;
            background: linear-gradient(135deg, #eb3349 0%, #f45c43 100%);
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 25px;
            font-weight: 600;
            margin-top: 20px;
            transition: transform 0.3s, box-shadow 0.3s;
            border: none;
            cursor: pointer;
        }
        .button:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(0,0,0,0.2);
        }
        .footer {
            background: #f8f9fa;
            padding: 15px;
            text-align: center;
            font-size: 12px;
            color: #999;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='icon'>X</div>
            <h1>Orçamento Recusado</h1>
        </div>
        <div class='content'>
            <p>Sua ordem de serviço foi cancelada.</p>
            <p>Entre em contato para negociar um novo orçamento.</p>
        </div>
        <div class='footer'>
            <p>Oficina Mecânica - Atendimento de qualidade</p>
        </div>
    </div>
</body>
</html>", "text/html; charset=utf-8");
    }
}