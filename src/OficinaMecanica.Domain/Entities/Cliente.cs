using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public Documento Documento { get; private set; }
    public string Telefone { get; private set; }
    public Email Email { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public bool Ativo { get; private set; }

    // Relacionamento
    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();

    public Cliente(string nome, Documento documento, string telefone, Email email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório");

        if (documento is null)
            throw new ArgumentException("Documento é obrigatório");

        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone é obrigatório");

        if (email is null)
            throw new ArgumentException("Email é obrigatório");

        Id = Guid.NewGuid();
        Nome = nome;
        Documento = documento;
        Telefone = telefone;
        Email = email;
        CriadoEm = DateTime.UtcNow;
        Ativo = true;
    }

    // Construtor parameterless privado para EF Core
    private Cliente()
    {
        Nome = string.Empty;
        Telefone = string.Empty;
        Documento = null!;
        Email = null!;
    }

    public void Atualizar(string nome, string telefone, Email email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório");

        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone é obrigatório");

        if (email is null)
            throw new ArgumentException("Email é obrigatório");

        Nome = nome;
        Telefone = telefone;
        Email = email;
    }

    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;
}
