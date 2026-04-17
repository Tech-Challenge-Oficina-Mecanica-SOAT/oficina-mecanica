using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly ApplicationDbContext _context;

    public VeiculoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Veiculo?> GetByIdAsync(Guid id) => 
        await _context.Veiculos
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.Id == id);


    public async Task<IEnumerable<Veiculo>> GetAllAsync() =>
        await _context.Veiculos
            .Include(v => v.Cliente)
            .ToListAsync();


    public async Task<IEnumerable<Veiculo>> GetByClienteIdAsync(Guid clienteId) =>
        await _context.Veiculos
            .Include(v => v.Cliente)
            .Where(v => v.ClienteId == clienteId)
            .ToListAsync();


    public async Task<Veiculo?> GetByPlacaAsync(string placa)
    {
        var placaLimpa = new string(placa.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray()).ToUpper();
        return await _context.Veiculos
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.Placa == placaLimpa);
    }


    public async Task<Veiculo> AddAsync(Veiculo veiculo)
    {
        await _context.Veiculos.AddAsync(veiculo);
        await _context.SaveChangesAsync();
        return veiculo;
    }

    public async Task UpdateAsync(Veiculo veiculo)
    {
        _context.Entry(veiculo).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var veiculo = await GetByIdAsync(id);
        if (veiculo != null)
        {
            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByPlacaAsync(string placa)
    {
        var placaLimpa = new string(placa.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray()).ToUpper();
        return await _context.Veiculos.AnyAsync(v => v.Placa == placaLimpa);
    }

    public async Task<bool> ExistsByPlacaForOtherClienteAsync(string placa, Guid clienteId, Guid? veiculoId = null)
    {
        var placaLimpa = new string(placa.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray()).ToUpper();
        
        var query = _context.Veiculos.Where(v => v.Placa == placaLimpa && v.ClienteId != clienteId);
        
        if (veiculoId.HasValue)
            query = query.Where(v => v.Id != veiculoId.Value);
            
        return await query.AnyAsync();
    }
}
