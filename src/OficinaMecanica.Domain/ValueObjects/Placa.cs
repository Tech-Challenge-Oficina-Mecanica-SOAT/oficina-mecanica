using System.Text.RegularExpressions;

namespace OficinaMecanica.Domain.ValueObjects;

public sealed record Placa
{
    private static readonly Regex FormatoAntigo = new(@"^[A-Z]{3}[0-9]{4}$");
    private static readonly Regex FormatoMercosul = new(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$");

    public string Valor { get; }

    public Placa(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Placa invalida.", nameof(valor));

        var normalizada = new string(valor.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
        if (!FormatoAntigo.IsMatch(normalizada) && !FormatoMercosul.IsMatch(normalizada))
            throw new ArgumentException("Placa invalida.", nameof(valor));

        Valor = normalizada;
    }

    public override string ToString() => Valor;
    public static implicit operator string(Placa p) => p.Valor;
}
