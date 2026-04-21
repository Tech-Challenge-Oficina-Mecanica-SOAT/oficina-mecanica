using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task AdicionarAsync(Usuario usuario);
}
