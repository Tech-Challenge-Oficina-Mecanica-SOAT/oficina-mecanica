using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Domain.Entities;

public class Veiculo
{
    public Guid Id { get; private set; }
    public Placa Placa { get; private set; } = null!;
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public int Ano { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public Guid ClienteId { get; private set; }
    public Cliente? Cliente { get; private set; }
    public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
    private Veiculo() { }

    public Veiculo(Guid clienteId, Placa placa, string marca, string modelo, int ano)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("ClienteId é obrigatório");

        if (placa is null)
            throw new ArgumentException("Placa é obrigatória");

        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("Marca é obrigatória");

        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo é obrigatório");

        if (ano < 1900 || ano > DateTime.Now.Year + 1)
            throw new ArgumentException("Ano inválido");

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        Placa = placa;
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
        CriadoEm = DateTime.UtcNow;
    }

    public void Atualizar(Guid? clienteId, Placa placa, string marca, string modelo, int ano)
    {
        if (placa is null)
            throw new ArgumentException("Placa é obrigatória");

        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("Marca é obrigatória");

        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo é obrigatório");

        if (ano < 1900 || ano > DateTime.Now.Year + 1)
            throw new ArgumentException("Ano inválido");

        if (clienteId.HasValue && clienteId.Value != Guid.Empty)
            ClienteId = clienteId.Value;

        Placa = placa;
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
    }
}
