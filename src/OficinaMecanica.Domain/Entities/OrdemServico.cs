using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Events;

namespace OficinaMecanica.Domain.Entities;

public enum EnumStatusOS
{
    Recebida = 1,
    EmDiagnostico = 2,
    AguardandoAprovacao = 3,
    EmExecucao = 4,
    Finalizada = 5,
    Entregue = 6,
    Rejeitada = 7
}

public class OrdemServico : Entity
{
    public Guid Id { get; private set; }
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataFechamento { get; private set; }
    public EnumStatusOS StatusOS { get; private set; }
    public string Observacoes { get; private set; } = string.Empty;
    public decimal Total { get; private set; }
    public string? TokenAprovacao { get; set; }
    public bool TokenUsado { get; set; }

    public Guid ClienteId { get; private set; }
    public Cliente Cliente { get; set; } = null!;
    public Guid VeiculoId { get; private set; }
    public Veiculo Veiculo { get; set; } = null!;

    public ICollection<OrdemServicoPeca> Pecas { get; set; } = new List<OrdemServicoPeca>();
    public ICollection<OrdemServicoServico> Servicos { get; set; } = new List<OrdemServicoServico>();
    public ICollection<OrdemServicoItem> Itens { get; set; } = new List<OrdemServicoItem>();
    public ICollection<HistoricoStatusOS> Historico { get; private set; } = new List<HistoricoStatusOS>();

    public OrdemServico() { }

    public OrdemServico(Guid clienteId, Guid veiculoId, string observacoes)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("Cliente é obrigatório.");
        if (veiculoId == Guid.Empty)
            throw new ArgumentException("Veiculo é obrigatório.");

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        VeiculoId = veiculoId;
        Observacoes = observacoes ?? string.Empty;
        DataAbertura = DateTime.UtcNow;
        StatusOS = EnumStatusOS.Recebida;

        RegistrarHistorico(null, EnumStatusOS.Recebida, "sistema", "Criação inicial");
        RaiseEvent(new OrdemCriadaEvent(Id, Cliente?.Email?.Valor ?? string.Empty, DateTime.UtcNow));
    }

    public void IniciarDiagnostico(string alteradoPor)
    {
        Transitar(EnumStatusOS.Recebida, EnumStatusOS.EmDiagnostico, alteradoPor, "Diagnóstico iniciado");
        RaiseEvent(new DiagnosticoIniciadoEvent(Id, Cliente?.Email?.Valor ?? string.Empty, alteradoPor, DateTime.UtcNow));
    }

    public void EnviarParaAprovacao(string alteradoPor)
    {
        Transitar(EnumStatusOS.EmDiagnostico, EnumStatusOS.AguardandoAprovacao, alteradoPor, "Orçamento enviado, aguardando aprovação do cliente");
        RaiseEvent(new OrcamentoEnviadoEvent(Id, Cliente?.Email?.Valor ?? string.Empty, Total, DateTime.UtcNow));
    }

    public void Aprovar(string alteradoPor)
    {
        Transitar(EnumStatusOS.AguardandoAprovacao, EnumStatusOS.EmExecucao, alteradoPor, "Orçamento aprovado");
        RaiseEvent(new OrdemAprovadaEvent(Id, Cliente?.Email?.Valor ?? string.Empty, Total, alteradoPor, DateTime.UtcNow));    }

    public void Rejeitar(string alteradoPor, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Informe o motivo da rejeição.");
        Transitar(EnumStatusOS.AguardandoAprovacao, EnumStatusOS.Rejeitada, alteradoPor, $"Orçamento rejeitado: {motivo}");
        RaiseEvent(new OrdemRejeitadaEvent(Id, Cliente?.Email?.Valor ?? string.Empty, alteradoPor, motivo, DateTime.UtcNow));
    }

    public void Finalizar(string alteradoPor)
    {
        Transitar(EnumStatusOS.EmExecucao, EnumStatusOS.Finalizada, alteradoPor, "Execução finalizada");
        RaiseEvent(new OrdemConcluidaEvent(Id, Cliente?.Email?.Valor ?? string.Empty, alteradoPor, DateTime.UtcNow));
    }

    public void Entregar(string alteradoPor)
    {
        Transitar(EnumStatusOS.Finalizada, EnumStatusOS.Entregue, alteradoPor, "Veículo entregue ao cliente");
        DataFechamento = DateTime.UtcNow;
        RaiseEvent(new OrdemEntregueEvent(Id, Cliente?.Email?.Valor ?? string.Empty, alteradoPor, DateTime.UtcNow));
    }

    public void ForcarStatus(EnumStatusOS novoStatus, string alteradoPor, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Informe o motivo do override.");
        if (novoStatus == StatusOS)
            throw new ArgumentException("Novo status é igual ao atual.");

        var anterior = StatusOS;
        StatusOS = novoStatus;
        if (novoStatus == EnumStatusOS.Entregue)
            DataFechamento = DateTime.UtcNow;

        RegistrarHistorico(anterior, novoStatus, alteradoPor, $"Override administrativo: {motivo}");
    }

    public void RecalcularTotal()
    {
        Total = Itens.Sum(i => i.Subtotal);
    }

    public void AtualizarTotal(decimal valor)
    {
        Total = valor;
    }

    private void Transitar(EnumStatusOS esperadoAtual, EnumStatusOS novo, string alteradoPor, string motivo)
    {
        if (StatusOS != esperadoAtual)
            throw new InvalidOperationException(
                $"Transição inválida: status atual é {StatusOS}, esperado {esperadoAtual} para ir a {novo}.");

        var anterior = StatusOS;
        StatusOS = novo;
        RegistrarHistorico(anterior, novo, alteradoPor, motivo);
    }

    private void RegistrarHistorico(EnumStatusOS? anterior, EnumStatusOS novo, string alteradoPor, string motivo)
    {
        Historico.Add(new HistoricoStatusOS(Id, anterior, novo, alteradoPor, motivo));
    }
}
