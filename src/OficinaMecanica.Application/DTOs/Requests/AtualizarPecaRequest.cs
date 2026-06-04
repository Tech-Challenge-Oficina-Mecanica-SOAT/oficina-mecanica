namespace OficinaMecanica.Application.DTOs.Requests;
public class AtualizarPecaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Estoque { get; set; }
}
