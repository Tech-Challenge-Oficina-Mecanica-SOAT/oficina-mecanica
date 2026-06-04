namespace OficinaMecanica.Application.UseCases.Servico.AtualizarServico;

public record AtualizarServicoUseCaseRequest(Guid Id, string Nome, string Descricao, decimal Valor);
