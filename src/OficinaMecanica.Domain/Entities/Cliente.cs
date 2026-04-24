using System.Text;
using System.Text.RegularExpressions;

namespace OficinaMecanica.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Documento { get; private set; }
    public string Telefone { get; private set; }
    public string Email { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public bool Ativo { get; private set; }

    // Relacionamento
    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();

    public Cliente(string nome, string documento, string telefone, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório");
            
        if (!ValidarDocumento(documento))
            throw new ArgumentException("Documento inválido (CPF ou CNPJ)");
            
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone é obrigatório");
            
        if (!ValidarEmail(email))
            throw new ArgumentException("Email inválido");
        
        Id = Guid.NewGuid();
        Nome = nome;
        Documento = LimparDocumento(documento);
        Telefone = telefone;
        Email = email.ToLower().Trim();
        CriadoEm = DateTime.UtcNow;
        Ativo = true;
    }

    public void Atualizar(string nome, string telefone, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório");
            
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone é obrigatório");
            
        if (!ValidarEmail(email))
            throw new ArgumentException("Email inválido");
            
        Nome = nome;
        Telefone = telefone;
        Email = email.ToLower().Trim();
    }

    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;
    
    private static string LimparDocumento(string documento)
    {
        // Remove caracteres não numéricos e letras
        return new string(documento.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpper();
    }
    
    private static bool ValidarDocumento(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return false;
            
        var docLimpo = LimparDocumento(documento);
        
        if (docLimpo.Length == 11)
            return ValidarCpf(docLimpo);
        else if (docLimpo.Length == 14)
            return ValidarCnpj(docLimpo);
        else
            return false;
    }
    
    private static bool ValidarCpf(string cpf)
    {
        // Verificar se todos os dígitos são iguais
        if (cpf.Distinct().Count() == 1)
            return false;
            
        int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        
        string tempCpf = cpf.Substring(0, 9);
        int soma = tempCpf.Select((c, i) => int.Parse(c.ToString()) * multiplicador1[i]).Sum();
        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;
        
        tempCpf += digito1;
        soma = tempCpf.Select((c, i) => int.Parse(c.ToString()) * multiplicador2[i]).Sum();
        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;
        
        return cpf.EndsWith($"{digito1}{digito2}");
    }
    
    private static bool ValidarCnpj(string cnpj)
    {
        // Verificar se todos os dígitos são iguais
        if (cnpj.Distinct().Count() == 1)
            return false;
            
        // CNPJ alfanumérico (formato Mercosul)
        if (cnpj.Length == 14 && cnpj.Any(c => char.IsLetter(c)))
        {
            return ValidarCnpjAlfanumerico(cnpj);
        }
        
        // CNPJ numérico tradicional
        if (cnpj.Length == 14 && cnpj.All(c => char.IsDigit(c)))
        {
            return ValidarCnpjNumerico(cnpj);
        }
        
        return false;
    }
    
    private static bool ValidarCnpjNumerico(string cnpj)
    {
        int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        
        string tempCnpj = cnpj.Substring(0, 12);
        int soma = tempCnpj.Select((c, i) => int.Parse(c.ToString()) * multiplicador1[i]).Sum();
        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;
        
        tempCnpj += digito1;
        soma = tempCnpj.Select((c, i) => int.Parse(c.ToString()) * multiplicador2[i]).Sum();
        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;
        
        return cnpj.EndsWith($"{digito1}{digito2}");
    }
    
    private static bool ValidarCnpjAlfanumerico(string cnpj)
    {
        // Tabela de conversão para CNPJ alfanumérico (Mercosul)
        // Letras são convertidas em números: A=0, B=1, C=2, ..., J=9
        var cnpjNumerico = new StringBuilder();
        
        foreach (char c in cnpj)
        {
            if (char.IsDigit(c))
            {
                cnpjNumerico.Append(c);
            }
            else if (char.IsLetter(c))
            {
                // Converte letra para número (A=0, B=1, ..., J=9)
                int valor = char.ToUpper(c) - 'A';
                if (valor >= 0 && valor <= 9)
                {
                    cnpjNumerico.Append(valor);
                }
                else
                {
                    return false; // Letra inválida para CNPJ Mercosul
                }
            }
            else
            {
                return false; // Caractere inválido
            }
        }
        
        // Validar como CNPJ numérico
        return ValidarCnpjNumerico(cnpjNumerico.ToString());
    }
    
    private static bool ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
}
