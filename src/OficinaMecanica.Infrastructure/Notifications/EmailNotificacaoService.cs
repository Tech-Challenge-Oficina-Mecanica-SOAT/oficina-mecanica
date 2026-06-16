using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Configuration;

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

    public async Task EnviarOrcamentoAsync(Guid osId, string emailCliente, decimal totalOrcamento)
    {
        var os = await _ordemServicoRepository.ObterPorIdAsync(osId);
        if (os == null)
            throw new Exception($"Ordem de Serviço {osId} não encontrada");

        string? token = os.TokenAprovacao;
        
        if (string.IsNullOrEmpty(token) || os.TokenUsado)
        {
            token = Guid.NewGuid().ToString("N").Substring(0, 32);
            await _ordemServicoRepository.AtualizarTokenAsync(osId, token);
        }

        // Aqui 'token' não é mais null porque foi gerado se era null
        var assunto = $"Orçamento - OS #{osId.ToString().Substring(0, 8)}";
        
        var baseUrl = _emailSettings.BaseUrl.TrimEnd('/');
        var linkAprovar = $"{baseUrl}/api/webhooks/ordens-servico/aprovar/{token}?aprovado=true";
        var linkRecusar = $"{baseUrl}/api/webhooks/ordens-servico/aprovar/{token}?aprovado=false";
        
        var corpoHtml = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #333;'>Orçamento para sua Ordem de Serviço</h2>
                    <p><strong>OS #:</strong> {osId.ToString().Substring(0, 8)}</p>
                    <p><strong>Valor total:</strong> R$ {totalOrcamento:N2}</p>
                    <p><strong>Status:</strong> Aguardando Aprovação</p>
                    <hr/>
                    <p>Por favor, escolha uma das opções abaixo:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{linkAprovar}' style='background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 0 10px; display: inline-block;'>✅ APROVAR ORÇAMENTO</a>
                        <a href='{linkRecusar}' style='background-color: #dc3545; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 0 10px; display: inline-block;'>❌ RECUSAR ORÇAMENTO</a>
                    </div>
                    <hr/>
                    <p style='font-size: 12px; color: #666;'>Este link é de uso único. Após sua decisão, o status da OS será atualizado automaticamente.</p>
                    <p style='font-size: 12px; color: #666;'>Se os botões não funcionarem, copie e cole os links abaixo:</p>
                    <p style='font-size: 11px;'>Aprovar: {linkAprovar}</p>
                    <p style='font-size: 11px;'>Recusar: {linkRecusar}</p>
                </div>
            </body>
            </html>";
        
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);
    }

    public async Task EnviarAprovacaoAsync(Guid osId, string emailCliente)
    {
        var assunto = $"Orçamento Aprovado - OS #{osId.ToString()[..8]}";
        var corpoHtml = $@"
            <html><body style='font-family: Arial, sans-serif;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                <h2 style='color: #28a745;'>✅ Orçamento Aprovado!</h2>
                <p><strong>OS #:</strong> {osId.ToString()[..8]}</p>
                <p>Sua ordem de serviço foi aprovada e já entrou em execução.</p>
                <p>Acompanhe o status em tempo real:</p>
                <p><a href='{_emailSettings.BaseUrl.TrimEnd('/')}/Publico/os/{osId}/status'>Ver status da OS</a></p>
                <p>Obrigado pela confiança!</p>
            </div></body></html>";
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);
    }

    public async Task EnviarRejeicaoAsync(Guid osId, string emailCliente, string motivo)
    {
        var assunto = $"Orçamento Recusado - OS #{osId.ToString()[..8]}";
        var corpoHtml = $@"
            <html><body style='font-family: Arial, sans-serif;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                <h2 style='color: #dc3545;'>❌ Orçamento Recusado</h2>
                <p><strong>OS #:</strong> {osId.ToString()[..8]}</p>
                <p><strong>Motivo:</strong> {motivo}</p>
                <p>Entre em contato conosco para negociar um novo orçamento.</p>
            </div></body></html>";
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);
    }

    public async Task EnviarConclusaoAsync(Guid osId, string emailCliente)
    {
        var assunto = $"Serviço Concluído - OS #{osId.ToString()[..8]}";
        var corpoHtml = $@"
            <html><body style='font-family: Arial, sans-serif;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                <h2 style='color: #17a2b8;'>🔧 Serviço Concluído!</h2>
                <p><strong>OS #:</strong> {osId.ToString()[..8]}</p>
                <p>O serviço do seu veículo foi concluído. Entre em contato para agendar a retirada.</p>
                <p><a href='{_emailSettings.BaseUrl.TrimEnd('/')}/Publico/os/{osId}/status'>Ver status da OS</a></p>
            </div></body></html>";
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);
    }

    public async Task EnviarEntregaAsync(Guid osId, string emailCliente)
    {
        var assunto = $"Veículo Entregue - OS #{osId.ToString()[..8]}";
        var corpoHtml = $@"
            <html><body style='font-family: Arial, sans-serif;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                <h2 style='color: #28a745;'>🚗 Veículo Entregue!</h2>
                <p><strong>OS #:</strong> {osId.ToString()[..8]}</p>
                <p>Seu veículo foi entregue com sucesso. Obrigado pela preferência!</p>
                <p>Avalie nosso serviço e volte sempre.</p>
            </div></body></html>";
        await EnviarEmailAsync(emailCliente, assunto, corpoHtml);
    }
}