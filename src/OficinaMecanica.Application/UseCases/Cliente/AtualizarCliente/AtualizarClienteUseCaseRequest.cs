namespace OficinaMecanica.Application.UseCases.Cliente.AtualizarCliente;

public record AtualizarClienteUseCaseRequest(Guid Id, string Nome, string Telefone, string Email);
