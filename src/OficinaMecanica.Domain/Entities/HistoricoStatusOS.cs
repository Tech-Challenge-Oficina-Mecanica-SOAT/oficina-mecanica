namespace OficinaMecanica.Domain.Entities;

public class HistoricoStatusOS
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public EnumStatusOS? StatusAnterior { get; private set; }
    public EnumStatusOS StatusNovo { get; private set; }
    public DateTime AlteradoEm { get; private set; }
    public string AlteradoPor { get; private set; }
    public string Motivo { get; private set; }

    public OrdemServico OrdemServico { get; private set; } = null!;

    private HistoricoStatusOS() { AlteradoPor = string.Empty; Motivo = string.Empty; }

    public HistoricoStatusOS(
        Guid ordemServicoId,
        EnumStatusOS? statusAnterior,
        EnumStatusOS statusNovo,
        string alteradoPor,
        string motivo)
    {
        if (ordemServicoId == Guid.Empty)
            throw new ArgumentException("OrdemServicoId é obrigatório.");
        if (string.IsNullOrWhiteSpace(alteradoPor))
            throw new ArgumentException("AlteradoPor é obrigatório.");
        if (statusAnterior.HasValue && statusAnterior.Value == statusNovo)
            throw new ArgumentException("StatusAnterior e StatusNovo não podem ser o mesmo.");

        Id = Guid.NewGuid();
        OrdemServicoId = ordemServicoId;
        StatusAnterior = statusAnterior;
        StatusNovo = statusNovo;
        AlteradoEm = DateTime.UtcNow;
        AlteradoPor = alteradoPor;
        Motivo = motivo ?? string.Empty;
    }
}
