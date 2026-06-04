namespace OficinaMecanica.Application.DTOs.Responses;
public class OrdemServicoResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public Guid VeiculoId { get; set; }
    public string VeiculoDescricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public List<OrdemServicoItemResponse> Itens { get; set; } = new();
}
