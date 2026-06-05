namespace OficinaMecanica.Application.DTOs.Requests;
public class AdicionarOSItemRequest
{
    public string Tipo { get; set; } = string.Empty;
    public Guid ReferenciaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}
