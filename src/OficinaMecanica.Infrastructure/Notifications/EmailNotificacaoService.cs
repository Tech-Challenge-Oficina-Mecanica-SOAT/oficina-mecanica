using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Configuration;
using System.Text;

namespace OficinaMecanica.Infrastructure.Notifications;

public class EmailNotificacaoService : INotificacaoService
{
    private readonly EmailSettings _emailSettings;
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IAppLogger<EmailNotificacaoService> _logger;

    public EmailNotificacaoService(
        IOptions<EmailSettings> emailSettings,
        IOrdemServicoRepository ordemServicoRepository,
        IAppLogger<EmailNotificacaoService> logger)
    {
        _emailSettings = emailSettings.Value;
        _ordemServicoRepository = ordemServicoRepository;
        _logger = logger;
    }

    private async Task EnviarEmailAsync(string para, string assunto, string corpoHtml)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Oficina Mecânica", _emailSettings.From));
        message.To.Add(new MailboxAddress("", para));
        message.Subject = assunto;
        message.Body = new TextPart("html") { Text = corpoHtml };

        using var client = new SmtpClient();
        await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, 
            _emailSettings.UseSSL);
        
        if (!string.IsNullOrEmpty(_emailSettings.UserName))
            await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
        
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task EnviarCriacaoAsync(Guid osId, string emailCliente)
    {
        if (string.IsNullOrEmpty(emailCliente))
        {
            _logger.Info($"Email nao enviado - OS: {osId}, Cliente sem email cadastrado");
            return;
        }

        var os = await _ordemServicoRepository.ObterPorIdComItensAsync(osId);
        if (os == null)
        {
            _logger.Info($"OS {osId} nao encontrada");
            return;
        }

        var nomeCliente = os.Cliente?.Nome ?? "Cliente";
        var veiculoInfo = os.Veiculo != null 
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} (Placa: {os.Veiculo.Placa?.Valor ?? "Nao informado"})" 
            : "Nao informado";

        var assunto = $"Ordem de Servico Aberta - OS #{osId.ToString().Substring(0, 8)}";
        
        var corpoHtml = $@"
            <!DOCTYPE html>
            <html lang='pt-BR'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; }}
                    .header {{ text-align: center; border-bottom: 2px solid #007bff; margin-bottom: 20px; }}
                    .header h2 {{ color: #007bff; }}
                    .info-box {{ background-color: #fff; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #ddd; }}
                    .footer {{ font-size: 12px; color: #666; text-align: center; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Ordem de Serviço Aberta</h2>
                    </div>
                    
                    <p>Olá <strong>{nomeCliente}</strong>,</p>
                    
                    <p>Informamos que uma Ordem de Serviço foi aberta para o seu veículo.</p>
                    
                    <div class='info-box'>
                        <p><strong>Número da OS:</strong> {osId.ToString().Substring(0, 8)}</p>
                        <p><strong>Veículo:</strong> {veiculoInfo}</p>
                        <p><strong>Status atual:</strong> <span style='color: #007bff;'>Recebida</span></p>
                    </div>
                    
                    <p>Em breve o diagnóstico será iniciado e você receberá o orçamento para aprovação.</p>
                    
                    <hr/>
                    
                    <div class='footer'>
                        <p>Este é um e-mail automático. Por favor, não responda.<br/>
                        Em caso de dúvidas, entre em contato com a oficina.</p>
                    </div>
                </div>
            </body>
            </html>";
        
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);
    }

    public async Task EnviarDiagnosticoAsync(Guid osId, string emailCliente)
    {
        if (string.IsNullOrEmpty(emailCliente))
        {
            _logger.Info($"Email nao enviado - OS: {osId}, Cliente sem email cadastrado");
            return;
        }

        var os = await _ordemServicoRepository.ObterPorIdComItensAsync(osId);
        if (os == null)
        {
            _logger.Info($"OS {osId} nao encontrada");
            return;
        }

        var nomeCliente = os.Cliente?.Nome ?? "Cliente";

        var assunto = $"Diagnostico Iniciado - OS #{osId.ToString().Substring(0, 8)}";
        
        var corpoHtml = $@"
            <!DOCTYPE html>
            <html lang='pt-BR'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; }}
                    .header {{ text-align: center; border-bottom: 2px solid #17a2b8; margin-bottom: 20px; }}
                    .header h2 {{ color: #17a2b8; }}
                    .info-box {{ background-color: #fff; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #ddd; }}
                    .footer {{ font-size: 12px; color: #666; text-align: center; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Diagnóstico Iniciado</h2>
                    </div>
                    
                    <p>Olá <strong>{nomeCliente}</strong>,</p>
                    
                    <p>Informamos que o diagnóstico da sua Ordem de Serviço foi iniciado.</p>
                    
                    <div class='info-box'>
                        <p><strong>Número da OS:</strong> {osId.ToString().Substring(0, 8)}</p>
                        <p><strong>Status atual:</strong> <span style='color: #17a2b8;'>Em Diagnóstico</span></p>
                    </div>
                    
                    <p>Em breve você receberá o orçamento para aprovação.</p>
                    
                    <hr/>
                    
                    <div class='footer'>
                        <p>Este é um e-mail automático. Por favor, não responda.<br/>
                        Em caso de dúvidas, entre em contato com a oficina.</p>
                    </div>
                </div>
            </body>
            </html>";
        
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);
    }

    public async Task EnviarOrcamentoAsync(Guid osId, string emailCliente, decimal totalOrcamento)
    {
        var os = await _ordemServicoRepository.ObterPorIdComItensAsync(osId);
        if (os == null)
            throw new Exception($"Ordem de Serviço {osId} não encontrada");

        string? token = os.TokenAprovacao;
        
        if (string.IsNullOrEmpty(token) || os.TokenUsado)
        {
            token = Guid.NewGuid().ToString("N").Substring(0, 32);
            await _ordemServicoRepository.AtualizarTokenAsync(osId, token);
        }

        var nomeCliente = os.Cliente?.Nome ?? "Cliente";
        var veiculoInfo = os.Veiculo != null 
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} (Placa: {os.Veiculo.Placa?.Valor ?? "Nao informado"})" 
            : "Nao informado";

        // Monta a tabela de itens
        var itensHtml = new StringBuilder();
        itensHtml.Append(@"
            <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
                <thead>
                    <tr>
                        <th style='border: 1px solid #ddd; padding: 8px; background-color: #f2f2f2; text-align: left;'>Item</th>
                        <th style='border: 1px solid #ddd; padding: 8px; background-color: #f2f2f2; text-align: center;'>Quantidade</th>
                        <th style='border: 1px solid #ddd; padding: 8px; background-color: #f2f2f2; text-align: right;'>Valor Unitário</th>
                        <th style='border: 1px solid #ddd; padding: 8px; background-color: #f2f2f2; text-align: right;'>Subtotal</th>
                    </tr>
                </thead>
                <tbody>");

        foreach (var item in os.Itens)
        {
            itensHtml.Append($@"
                <tr>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{item.Descricao}</td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{item.Quantidade}</td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>R$ {item.PrecoUnitario:N2}</td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>R$ {item.Subtotal:N2}</td>
                </tr>");
        }

        itensHtml.Append(@"
                </tbody>
            </table>");

        var assunto = $"Orçamento - OS #{osId.ToString().Substring(0, 8)}";
        
        var baseUrl = _emailSettings.BaseUrl.TrimEnd('/');
        var linkAprovar = $"{baseUrl}/api/webhooks/ordens-servico/aprovar/{token}?aprovado=true";
        var linkRecusar = $"{baseUrl}/api/webhooks/ordens-servico/aprovar/{token}?aprovado=false";
        
        var corpoHtml = $@"
            <!DOCTYPE html>
            <html lang='pt-BR'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                    .container {{ max-width: 800px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; }}
                    .header {{ text-align: center; border-bottom: 2px solid #007bff; margin-bottom: 20px; }}
                    .header h2 {{ color: #007bff; }}
                    .info-box {{ background-color: #fff; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #ddd; }}
                    .button {{ display: inline-block; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 0 10px; }}
                    .button-approve {{ background-color: #28a745; color: white; }}
                    .button-reject {{ background-color: #dc3545; color: white; }}
                    .footer {{ font-size: 12px; color: #666; text-align: center; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Orçamento para sua Ordem de Serviço</h2>
                    </div>
                    
                    <p>Olá <strong>{nomeCliente}</strong>,</p>
                    
                    <p>Segue abaixo o detalhamento do orçamento para o seu veículo <strong>{veiculoInfo}</strong>:</p>
                    
                    <div class='info-box'>
                        <p><strong>Número da OS:</strong> {osId.ToString().Substring(0, 8)}</p>
                        
                        {itensHtml.ToString()}
                        
                        <div style='text-align: right; margin-top: 15px; font-weight: bold;'>
                            <p><strong>TOTAL: R$ {totalOrcamento:N2}</strong></p>
                        </div>
                        
                        <p><strong>Status:</strong> <span style='color: #007bff;'>Aguardando Aprovação</span></p>
                    </div>
                    
                    <p>Por favor, escolha uma das opções abaixo para aprovar ou recusar o orçamento:</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{linkAprovar}' class='button button-approve'>APROVAR ORÇAMENTO</a>
                        <a href='{linkRecusar}' class='button button-reject'>RECUSAR ORÇAMENTO</a>
                    </div>
                    
                    <hr/>
                    
                    <div class='footer'>
                        <p>Este é um e-mail automático. Por favor, não responda.<br/>
                        Em caso de dúvidas, entre em contato com a oficina.<br/>
                        Este link é de uso único. Após sua decisão, o status da OS será atualizado automaticamente.</p>
                    </div>
                </div>
            </body>
            </html>";
        
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);
    }

    public async Task EnviarAprovacaoAsync(Guid osId, string emailCliente)
    {
        var os = await _ordemServicoRepository.ObterPorIdComItensAsync(osId);
        if (os == null)
            throw new Exception($"Ordem de Serviço {osId} não encontrada");

        var nomeCliente = os.Cliente?.Nome ?? "Cliente";
        var veiculoInfo = os.Veiculo != null 
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} (Placa: {os.Veiculo.Placa?.Valor ?? "Nao informado"})" 
            : "Nao informado";

        var assunto = $"Serviço Concluído - OS #{osId.ToString().Substring(0, 8)}";
        
        var corpoHtml = $@"
            <!DOCTYPE html>
            <html lang='pt-BR'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; }}
                    .header {{ text-align: center; border-bottom: 2px solid #28a745; margin-bottom: 20px; }}
                    .header h2 {{ color: #28a745; }}
                    .info-box {{ background-color: #fff; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #ddd; }}
                    .footer {{ font-size: 12px; color: #666; text-align: center; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Serviço Concluído</h2>
                    </div>
                    
                    <p>Olá <strong>{nomeCliente}</strong>,</p>
                    
                    <p>Informamos que o serviço do seu veículo <strong>{veiculoInfo}</strong> foi <strong style='color: #28a745;'>CONCLUÍDO</strong> com sucesso.</p>
                    
                    <div class='info-box'>
                        <p><strong>Número da OS:</strong> {osId.ToString().Substring(0, 8)}</p>
                        <p><strong>Valor total:</strong> R$ {os.Total:N2}</p>
                        <p><strong>Status atual:</strong> <span style='color: #28a745;'>Finalizada</span></p>
                    </div>
                    
                    <p>Seu veículo está pronto para retirada. Fique à vontade para agendar a retirada em horário comercial.</p>
                    
                    <hr/>
                    
                    <div class='footer'>
                        <p>Este é um e-mail automático. Por favor, não responda.<br/>
                        Em caso de dúvidas, entre em contato com a oficina.</p>
                    </div>
                </div>
            </body>
            </html>";
        
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);

        _logger.Info($"Notificação de conclusão - OS: {osId}, Cliente: {emailCliente}");
    }

    public async Task EnviarRejeicaoAsync(Guid osId, string emailCliente, string motivo)
    {
        var os = await _ordemServicoRepository.ObterPorIdComItensAsync(osId);
        if (os == null)
            throw new Exception($"Ordem de Serviço {osId} não encontrada");

        var nomeCliente = os.Cliente?.Nome ?? "Cliente";
        var veiculoInfo = os.Veiculo != null 
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} (Placa: {os.Veiculo.Placa?.Valor ?? "Nao informado"})" 
            : "Nao informado";

        var assunto = $"Orçamento Aprovado - OS #{osId.ToString().Substring(0, 8)}";
        
        var corpoHtml = $@"
            <!DOCTYPE html>
            <html lang='pt-BR'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; }}
                    .header {{ text-align: center; border-bottom: 2px solid #28a745; margin-bottom: 20px; }}
                    .header h2 {{ color: #28a745; }}
                    .info-box {{ background-color: #fff; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #ddd; }}
                    .footer {{ font-size: 12px; color: #666; text-align: center; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Orçamento Aprovado</h2>
                    </div>
                    
                    <p>Olá <strong>{nomeCliente}</strong>,</p>
                    
                    <p>Informamos que o orçamento do seu veículo <strong>{veiculoInfo}</strong> foi <strong style='color: #28a745;'>APROVADO</strong>.</p>
                    
                    <div class='info-box'>
                        <p><strong>Número da OS:</strong> {osId.ToString().Substring(0, 8)}</p>
                        <p><strong>Valor total:</strong> R$ {os.Total:N2}</p>
                        <p><strong>Status atual:</strong> <span style='color: #28a745;'>Em Execução</span></p>
                    </div>
                    
                    <p>O serviço já está em andamento. Você receberá novas notificações quando o serviço for concluído.</p>
                    
                    <hr/>
                    
                    <div class='footer'>
                        <p>Este é um e-mail automático. Por favor, não responda.<br/>
                        Em caso de dúvidas, entre em contato com a oficina.</p>
                    </div>
                </div>
            </body>
            </html>";
        
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);

        _logger.Info($"Orçamento aprovado - OS: {osId}, Cliente: {emailCliente}");
    }

    public async Task EnviarConclusaoAsync(Guid osId, string emailCliente, string motivo)
    {
        var os = await _ordemServicoRepository.ObterPorIdComItensAsync(osId);
        if (os == null)
            throw new Exception($"Ordem de Serviço {osId} não encontrada");

        var nomeCliente = os.Cliente?.Nome ?? "Cliente";
        var veiculoInfo = os.Veiculo != null 
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} (Placa: {os.Veiculo.Placa?.Valor ?? "Nao informado"})" 
            : "Nao informado";

        var assunto = $"Orçamento Rejeitado - OS #{osId.ToString().Substring(0, 8)}";
        
        var corpoHtml = $@"
            <!DOCTYPE html>
            <html lang='pt-BR'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; }}
                    .header {{ text-align: center; border-bottom: 2px solid #dc3545; margin-bottom: 20px; }}
                    .header h2 {{ color: #dc3545; }}
                    .info-box {{ background-color: #fff; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #ddd; }}
                    .footer {{ font-size: 12px; color: #666; text-align: center; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Orçamento Rejeitado</h2>
                    </div>
                    
                    <p>Olá <strong>{nomeCliente}</strong>,</p>
                    
                    <p>Informamos que o orçamento do seu veículo <strong>{veiculoInfo}</strong> foi <strong style='color: #dc3545;'>REJEITADO</strong>.</p>
                    
                    <div class='info-box'>
                        <p><strong>Número da OS:</strong> {osId.ToString().Substring(0, 8)}</p>
                        <p><strong>Valor total:</strong> R$ {os.Total:N2}</p>
                        <p><strong>Motivo da rejeição:</strong> {motivo}</p>
                        <p><strong>Status atual:</strong> <span style='color: #dc3545;'>Rejeitada</span></p>
                    </div>
                    
                    <p>A ordem de serviço foi cancelada. Entre em contato conosco para negociar um novo orçamento ou esclarecer dúvidas.</p>
                    
                    <hr/>
                    
                    <div class='footer'>
                        <p>Este é um e-mail automático. Por favor, não responda.<br/>
                        Em caso de dúvidas, entre em contato com a oficina.</p>
                    </div>
                </div>
            </body>
            </html>";
        
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);

        _logger.Info($"Orçamento rejeitado - OS: {osId}, Cliente: {emailCliente}, Motivo: {motivo}");
    }

    public async Task EnviarEntregaAsync(Guid osId, string emailCliente)
    {
        var os = await _ordemServicoRepository.ObterPorIdComItensAsync(osId);
        if (os == null)
            throw new Exception($"Ordem de Serviço {osId} não encontrada");

        var nomeCliente = os.Cliente?.Nome ?? "Cliente";
        var veiculoInfo = os.Veiculo != null 
            ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo} (Placa: {os.Veiculo.Placa?.Valor ?? "Nao informado"})" 
            : "Nao informado";

        var assunto = $"Veículo Entregue - OS #{osId.ToString().Substring(0, 8)}";
        
        var corpoHtml = $@"
            <!DOCTYPE html>
            <html lang='pt-BR'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; }}
                    .header {{ text-align: center; border-bottom: 2px solid #28a745; margin-bottom: 20px; }}
                    .header h2 {{ color: #28a745; }}
                    .info-box {{ background-color: #fff; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #ddd; }}
                    .footer {{ font-size: 12px; color: #666; text-align: center; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Veículo Entregue</h2>
                    </div>
                    
                    <p>Olá <strong>{nomeCliente}</strong>,</p>
                    
                    <p>Informamos que o serviço do seu veículo <strong>{veiculoInfo}</strong> foi concluído e o veículo já foi retirado da oficina.</p>
                    
                    <div class='info-box'>
                        <p><strong>Número da OS:</strong> {osId.ToString().Substring(0, 8)}</p>
                        <p><strong>Valor total:</strong> R$ {os.Total:N2}</p>
                        <p><strong>Status atual:</strong> <span style='color: #28a745;'>Entregue</span></p>
                    </div>
                    
                    <p>Agradecemos pela confiança e preferência. Estamos à disposição para futuros serviços.</p>
                    
                    <hr/>
                    
                    <div class='footer'>
                        <p>Este é um e-mail automático. Por favor, não responda.<br/>
                        Em caso de dúvidas, entre em contato com a oficina.</p>
                    </div>
                </div>
            </body>
            </html>";
        
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);

        _logger.Info($"Veículo entregue - OS: {osId}, Cliente: {emailCliente}");
    }
}