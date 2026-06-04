namespace OficinaMecanica.Application.DTOs.Requests;
public class AbrirOrdemServicoRequest
{
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string Observacoes { get; set; } = string.Empty;
}
