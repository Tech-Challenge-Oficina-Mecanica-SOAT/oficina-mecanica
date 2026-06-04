namespace OficinaMecanica.Application.UseCases.Peca.AtualizarPeca;

public record AtualizarPecaUseCaseRequest(Guid Id, string Nome, string Descricao, decimal PrecoUnitario, int Estoque);
