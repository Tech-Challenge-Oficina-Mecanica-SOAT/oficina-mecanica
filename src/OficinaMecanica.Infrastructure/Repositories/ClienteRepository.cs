using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetByIdAsync(Guid id)
    {
        return await _context.Clientes.FindAsync(id);
    }

    public async Task<Cliente?> GetByDocumentoAsync(string documento)
    {
        // Limpar o documento antes de buscar
        var docLimpo = new string(documento.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpper();
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Documento == docLimpo);
    }

    public async Task<IEnumerable<Cliente>> GetAllAsync()
    {
        return await _context.Clientes.ToListAsync();
    }

    public async Task<Cliente> AddAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task UpdateAsync(Cliente cliente)
    {
        _context.Entry(cliente).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var cliente = await GetByIdAsync(id);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByDocumentoAsync(string documento)
    {
        var docLimpo = new string(documento.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpper();
        return await _context.Clientes.AnyAsync(c => c.Documento == docLimpo);
    }
}
