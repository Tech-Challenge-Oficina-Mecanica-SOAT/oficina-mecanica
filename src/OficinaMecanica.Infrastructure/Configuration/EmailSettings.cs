namespace OficinaMecanica.Infrastructure.Configuration;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool UseSSL { get; set; } = false;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = "noreply@oficinamecanica.com";
}