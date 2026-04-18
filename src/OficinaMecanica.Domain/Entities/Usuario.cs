using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public Perfil Perfil { get; private set; }
    public Guid? ClienteId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public Usuario(string email, string senhaHash, Perfil perfil, Guid? clienteId = null)
    {
        Id = Guid.NewGuid();
        Email = email.ToLower().Trim();
        SenhaHash = senhaHash;
        Perfil = perfil;
        ClienteId = clienteId;
        CriadoEm = DateTime.UtcNow;
    }

    public void AtualizarSenha(string senhaHash) => SenhaHash = senhaHash;
}
