namespace OficinaMecanica.Application.DTOs.Requests;
public class CriarClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
