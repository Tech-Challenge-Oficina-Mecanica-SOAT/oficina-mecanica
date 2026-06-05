using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly ApplicationDbContext _context;

    public VeiculoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Veiculo?> GetByIdAsync(Guid id)
    {
        return await _context.Veiculos
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Veiculo?> GetByPlacaAsync(string placa)
    {
        var placaVO = new Placa(placa);
        return await _context.Veiculos
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.Placa == placaVO);
    }

    public async Task<IEnumerable<Veiculo>> GetAllAsync()
    {
        return await _context.Veiculos
            .Include(v => v.Cliente)
            .OrderBy(v => v.Placa)
            .ToListAsync();
    }

    public async Task<IEnumerable<Veiculo>> GetByClienteIdAsync(Guid clienteId)
    {
        return await _context.Veiculos
            .Include(v => v.Cliente)
            .Where(v => v.ClienteId == clienteId)
            .ToListAsync();
    }

    public async Task<Veiculo> AddAsync(Veiculo veiculo)
    {
        await _context.Veiculos.AddAsync(veiculo);
        await _context.SaveChangesAsync();
        return veiculo;
    }

    public async Task<Veiculo> UpdateAsync(Veiculo veiculo)
    {
        _context.Entry(veiculo).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return veiculo;
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
        var placaVO = new Placa(placa);
        return await _context.Veiculos.AnyAsync(v => v.Placa == placaVO);
    }

    public async Task<bool> ExistsByPlacaForOtherVeiculoAsync(string placa, Guid veiculoId)
    {
        var placaVO = new Placa(placa);
        return await _context.Veiculos.AnyAsync(v => v.Placa == placaVO && v.Id != veiculoId);
    }

    public async Task<bool> ClienteExistsAsync(Guid clienteId)
    {
        return await _context.Clientes.AnyAsync(c => c.Id == clienteId);
    }
}
