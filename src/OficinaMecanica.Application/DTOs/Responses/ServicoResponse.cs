namespace OficinaMecanica.Application.DTOs.Responses;
public class ServicoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
