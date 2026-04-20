using OficinaMecanica.Application.DTOs;

namespace OficinaMecanica.Application.Interfaces
{
    public interface IPecaService
    {
        Task<PecaDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<PecaDto>> GetAllAsync();
        Task<IEnumerable<PecaDto>> GetByEstoqueBaixoAsync(int limiteEstoque);
        Task<PecaDto> CreateAsync(CreatePecaDto createDto);
        Task<PecaDto?> UpdateAsync(Guid id, UpdatePecaDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> UpdateEstoqueAsync(Guid id, UpdateEstoqueDto updateEstoqueDto);
        Task<int> GetEstoqueAsync(Guid id);
    }
}
