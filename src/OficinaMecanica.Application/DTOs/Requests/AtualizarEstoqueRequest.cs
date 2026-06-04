namespace OficinaMecanica.Application.DTOs.Requests;
public class AtualizarEstoqueRequest
{
    public int Quantidade { get; set; }
    public string TipoOperacao { get; set; } = string.Empty;
}
