namespace OficinaMecanica.Application.DTOs;

public class PecaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Estoque { get; set; }
    public string? Descricao { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public bool Ativo { get; set; }
}

public class CreatePecaDto
{
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Estoque { get; set; }
    public string? Descricao { get; set; }
}

public class UpdatePecaDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Estoque { get; set; }
    public string? Descricao { get; set; }
}

public class UpdateEstoqueDto
{
    public int Quantidade { get; set; }
    public string TipoOperacao { get; set; } = string.Empty;
}
