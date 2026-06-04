using System.Text.RegularExpressions;

namespace OficinaMecanica.Domain.ValueObjects;

public sealed record Email
{
    public string Valor { get; }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) ||
            !Regex.IsMatch(valor.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Email inválido.", nameof(valor));
        Valor = valor.Trim().ToLower();
    }

    public override string ToString() => Valor;
    public static implicit operator string(Email e) => e.Valor;
}
