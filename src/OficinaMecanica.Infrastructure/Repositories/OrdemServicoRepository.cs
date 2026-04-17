using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure.Repositories;

public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly ApplicationDbContext _context;

    public OrdemServicoRepository(ApplicationDbContext context) => _context = context;

    public async Task<OrdemServico?> ObterPorIdAsync(Guid id) =>
        await _context.OrdensServico.FindAsync(id);
}
