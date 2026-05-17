using System.Text;

namespace OficinaMecanica.Domain.ValueObjects;

public sealed record Documento
{
    public string Valor { get; }
    public TipoDocumento Tipo { get; }

    public Documento(string valor)
    {
        var limpo = Limpar(valor ?? string.Empty);
        if (limpo.Length == 11 && ValidarCpf(limpo))
        {
            Tipo = TipoDocumento.Cpf;
            Valor = limpo;
            return;
        }
        if (limpo.Length == 14 && ValidarCnpj(limpo))
        {
            Tipo = TipoDocumento.Cnpj;
            Valor = limpo;
            return;
        }
        throw new ArgumentException("CPF ou CNPJ inválido.", nameof(valor));
    }

    public override string ToString() => Valor;
    public static implicit operator string(Documento d) => d.Valor;

    private static string Limpar(string s) =>
        new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpper();

    private static bool ValidarCpf(string cpf)
    {
        if (cpf.Distinct().Count() == 1) return false;
        int[] m1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var tmp = cpf.Substring(0, 9);
        var soma = tmp.Select((c, i) => int.Parse(c.ToString()) * m1[i]).Sum();
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        tmp += d1;
        soma = tmp.Select((c, i) => int.Parse(c.ToString()) * m2[i]).Sum();
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return cpf.EndsWith($"{d1}{d2}");
    }

    private static bool ValidarCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false;
        if (cnpj.Any(char.IsLetter))
            return ValidarCnpjAlfanumerico(cnpj);
        return cnpj.All(char.IsDigit) && ValidarCnpjNumerico(cnpj);
    }

    private static bool ValidarCnpjNumerico(string cnpj)
    {
        int[] m1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var tmp = cnpj.Substring(0, 12);
        var soma = tmp.Select((c, i) => int.Parse(c.ToString()) * m1[i]).Sum();
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        tmp += d1;
        soma = tmp.Select((c, i) => int.Parse(c.ToString()) * m2[i]).Sum();
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return cnpj.EndsWith($"{d1}{d2}");
    }

    private static bool ValidarCnpjAlfanumerico(string cnpj)
    {
        var sb = new StringBuilder();
        foreach (var c in cnpj)
        {
            if (char.IsDigit(c)) sb.Append(c);
            else if (char.IsLetter(c))
            {
                int v = char.ToUpper(c) - 'A';
                if (v < 0 || v > 9) return false;
                sb.Append(v);
            }
            else return false;
        }
        return ValidarCnpjNumerico(sb.ToString());
    }
}
