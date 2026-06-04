namespace OficinaMecanica.Application.UseCases.OrdemServicoStatus.RejeitarOS;

public record RejeitarOSUseCaseRequest(Guid OsId, string AlteradoPor, string Motivo);
