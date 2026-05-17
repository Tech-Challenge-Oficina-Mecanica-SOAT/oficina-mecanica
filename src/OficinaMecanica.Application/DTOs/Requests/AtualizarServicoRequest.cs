namespace OficinaMecanica.Application.DTOs.Requests;
public class AtualizarServicoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
