using OficinaMecanica.Application.DTOs;

namespace OficinaMecanica.Application.Interfaces;

public interface IPecaService
{
    Task<PecaDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PecaDto>> GetAllAsync();
    Task<IEnumerable<PecaDto>> GetByNomeAsync(string nome);
    Task<PecaDto> CreateAsync(CreatePecaDto createDto);
    Task<PecaDto?> UpdateAsync(Guid id, UpdatePecaDto updateDto);
    Task DeleteAsync(Guid id);
    Task<int> GetEstoqueAsync(Guid id);
    Task<PecaDto> UpdateEstoqueAsync(Guid id, UpdateEstoqueDto updateEstoqueDto);
    Task<IEnumerable<PecaDto>> GetByEstoqueBaixoAsync(int limiteEstoque);
}