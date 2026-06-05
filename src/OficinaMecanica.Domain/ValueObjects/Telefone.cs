namespace OficinaMecanica.Domain.ValueObjects;

public sealed record Telefone
{
    public string Valor { get; }

    public Telefone(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Telefone invalido.", nameof(valor));

        var apenasDigitos = new string(valor.Where(char.IsDigit).ToArray());
        if (apenasDigitos.Length < 10 || apenasDigitos.Length > 13)
            throw new ArgumentException("Telefone invalido.", nameof(valor));

        Valor = apenasDigitos;
    }

    public override string ToString() => Valor;
    public static implicit operator string(Telefone t) => t.Valor;
}
