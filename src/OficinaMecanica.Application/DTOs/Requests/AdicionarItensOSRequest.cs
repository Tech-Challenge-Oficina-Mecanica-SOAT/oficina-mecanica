namespace OficinaMecanica.Application.DTOs.Requests;

public class AdicionarItensOSRequest
{
    public Guid OrdemServicoId { get; set; }
    public List<AdicionarOSItemRequest> Itens { get; set; } = new();
}
