using System.Text.RegularExpressions;

namespace OficinaMecanica.Domain.Entities;

public class Veiculo
{
    public Guid Id { get; private set; }
    public string Placa { get; private set; } = null!;
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public int Ano { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public Guid ClienteId { get; private set; }
    public Cliente? Cliente { get; private set; }
    public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
    private Veiculo() { }

    public Veiculo(Guid clienteId, string placa, string marca, string modelo, int ano)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("ClienteId é obrigatório");

        if (!ValidarPlaca(placa))
            throw new ArgumentException("Placa inválida. Formato: ABC1234 ou ABC1D23");

        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("Marca é obrigatória");

        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo é obrigatório");

        if (ano < 1900 || ano > DateTime.Now.Year + 1)
            throw new ArgumentException("Ano inválido");

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        Placa = NormalizarPlaca(placa);
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
        CriadoEm = DateTime.UtcNow;
    }

    public void Atualizar(Guid? clienteId, string placa, string marca, string modelo, int ano)
    {
        if (!ValidarPlaca(placa))
            throw new ArgumentException("Placa inválida. Formato: ABC1234 ou ABC1D23");

        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("Marca é obrigatória");

        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo é obrigatório");

        if (ano < 1900 || ano > DateTime.Now.Year + 1)
            throw new ArgumentException("Ano inválido");

        if (clienteId.HasValue && clienteId.Value != Guid.Empty)
            ClienteId = clienteId.Value;

        Placa = NormalizarPlaca(placa);
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
    }

    private static string NormalizarPlaca(string placa)
    {
        return new string(placa.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray()).ToUpper();
    }

    private static bool ValidarPlaca(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
            return false;

        var placaLimpa = NormalizarPlaca(placa);

        // Formato antigo: ABC1234
        var padraoAntigo = @"^[A-Z]{3}[0-9]{4}$";
        // Formato Mercosul: ABC1D23
        var padraoMercosul = @"^[A-Z]{3}[0-9][A-Z][0-9]{2}$";

        return Regex.IsMatch(placaLimpa, padraoAntigo) || Regex.IsMatch(placaLimpa, padraoMercosul);
    }
}
