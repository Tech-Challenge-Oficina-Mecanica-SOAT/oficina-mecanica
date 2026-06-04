using System.Net.Mail;

namespace OficinaMecanica.Domain.ValueObjects;

public sealed record Email
{
    public string Valor { get; }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Email invalido.", nameof(valor));

        var trimmed = valor.Trim();
        if (!MailAddress.TryCreate(trimmed, out _))
            throw new ArgumentException("Email invalido.", nameof(valor));

        Valor = trimmed.ToLower();
    }

    public override string ToString() => Valor;
    public static implicit operator string(Email e) => e.Valor;
}
