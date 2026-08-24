namespace OficinaMecanica.API.Presentation.PainelStatus;

/// <summary>
/// Modelo de apresentação (view model) da página pública de acompanhamento de status.
/// Contém apenas dados já formatados e prontos para exibição — nenhuma regra de negócio
/// deve ser adicionada aqui. Isso mantém a camada de domínio livre de preocupações de UI.
/// </summary>
public sealed record PainelStatusViewModel(
    string OsIdCurto,
    string NomeCliente,
    string VeiculoInfo,
    string StatusLabel,
    string CorPrincipal,
    string CorSecundaria,
    string DataAberturaFormatada,
    string UltimaAtualizacaoFormatada,
    IReadOnlyList<PainelStatusTimelineItem> Timeline
);

/// <summary>
/// Um item do histórico de status exibido na timeline da página pública.
/// </summary>
public sealed record PainelStatusTimelineItem(
    string Cor,
    string Label,
    string DataFormatada,
    string? Motivo
);
