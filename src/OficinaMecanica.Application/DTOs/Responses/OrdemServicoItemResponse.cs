namespace OficinaMecanica.Application.DTOs.Responses;
public class OrdemServicoItemResponse
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public Guid ReferenciaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
