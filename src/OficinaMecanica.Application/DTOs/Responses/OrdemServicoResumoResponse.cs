namespace OficinaMecanica.Application.DTOs.Responses;
public class OrdemServicoResumoResponse
{
    public Guid Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string VeiculoDescricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
}
