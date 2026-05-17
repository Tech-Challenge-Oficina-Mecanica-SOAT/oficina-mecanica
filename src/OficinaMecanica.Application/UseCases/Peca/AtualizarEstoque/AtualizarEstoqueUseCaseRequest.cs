namespace OficinaMecanica.Application.UseCases.Peca.AtualizarEstoque;

public record AtualizarEstoqueUseCaseRequest(Guid Id, int Quantidade, string TipoOperacao);
