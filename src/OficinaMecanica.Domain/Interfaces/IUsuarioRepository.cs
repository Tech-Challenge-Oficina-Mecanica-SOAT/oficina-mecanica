using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario?> ObterPorClienteIdAsync(Guid clienteId);
    Task AdicionarAsync(Usuario usuario);
}
