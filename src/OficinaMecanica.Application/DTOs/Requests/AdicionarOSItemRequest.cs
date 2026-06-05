namespace OficinaMecanica.Application.DTOs.Requests;
public class AdicionarOSItemRequest
{
    public string Tipo { get; set; } = string.Empty;
    public Guid ReferenciaId { get; set; }
    public int Quantidade { get; set; }
}
