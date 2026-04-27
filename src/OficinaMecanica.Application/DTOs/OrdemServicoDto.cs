namespace OficinaMecanica.Application.DTOs;

// ─── OSItem ───────────────────────────────────────────────

public class OrdemServicoItemDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // "servico" | "peca" | "insumo"
    public Guid ReferenciaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class CreateOrdemServicoItemDto
{
    public string Tipo { get; set; } = string.Empty; // "servico" | "peca" | "insumo"
    public Guid ReferenciaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}

// ─── OrdemServico ─────────────────────────────────────────

public class OrdemServicoDto
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
    public List<OrdemServicoItemDto> Itens { get; set; } = new();
}

public class CreateOrdemServicoDto
{
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string Observacoes { get; set; } = string.Empty;
}

public class OrdemServicoResumoDto
{
    public Guid Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string VeiculoDescricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
}