namespace OficinaMecanica.Application.DTOs.Requests;
public class AtualizarClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
