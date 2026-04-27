using System;
using System.Collections.Generic;
using System.Text;

namespace OficinaMecanica.Domain.Entities;

public enum EnumStatusOS
{
    Recebida = 1,
    EmDiagnostico = 2,
    AguardandoAprovacao = 3,
    EmExecucao = 4,
    Finalizada = 5,
    Entregue = 6
}

public class OrdemServico
{
    public Guid Id { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public EnumStatusOS StatusOS { get; set; } = EnumStatusOS.EmDiagnostico;
    public string Observacoes { get; set; } = string.Empty;
    public decimal Total { get; set; }

    // FK
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public Guid VeiculoId { get; set; }
    public Veiculo Veiculo { get; set; } = null!;

    // Relacionamentos legados (mantidos para compatibilidade)
    public ICollection<OrdemServicoPeca> Pecas { get; set; } = new List<OrdemServicoPeca>();
    public ICollection<OrdemServicoServico> Servicos { get; set; } = new List<OrdemServicoServico>();

    // Novo relacionamento unificado
    public ICollection<OrdemServicoItem> Itens { get; set; } = new List<OrdemServicoItem>();

    public void RecalcularTotal()
    {
        Total = Itens.Sum(i => i.Subtotal);
    }
}