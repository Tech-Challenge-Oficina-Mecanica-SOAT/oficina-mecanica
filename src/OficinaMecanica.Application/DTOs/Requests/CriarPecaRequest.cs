namespace OficinaMecanica.Application.DTOs.Requests;
public class CriarPecaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Estoque { get; set; }
}
