using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Auth;

public class AutenticacaoClienteQuery : IAutenticacaoClienteQuery
{
    private readonly ApplicationDbContext _context;

    public AutenticacaoClienteQuery(ApplicationDbContext context) => _context = context;

    public async Task<DadosClienteAutenticacao?> ObterPorDocumentoAsync(string documento)
    {
        var docLimpo = new string(documento.Where(char.IsLetterOrDigit).ToArray()).ToUpper();

        return await _context.Clientes
            .Where(c => c.Documento == docLimpo)
            .Select(c => new DadosClienteAutenticacao(c.Id, c.Ativo))
            .FirstOrDefaultAsync();
    }
}
