namespace OficinaMecanica.Application.UseCases.Veiculo.AtualizarVeiculo;

public record AtualizarVeiculoUseCaseRequest(Guid Id, Guid? ClienteId, string Placa, string Marca, string Modelo, int Ano);
