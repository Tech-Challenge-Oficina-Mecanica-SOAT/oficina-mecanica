using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.Interfaces;

public interface IPecaService
{
    Task<PecaResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<PecaResponse>> GetAllAsync();
    Task<IEnumerable<PecaResponse>> GetByNomeAsync(string nome);
    Task<PecaResponse> CreateAsync(CriarPecaRequest createDto);
    Task<PecaResponse?> UpdateAsync(Guid id, AtualizarPecaRequest updateDto);
    Task DeleteAsync(Guid id);
    Task<int> GetEstoqueAsync(Guid id);
    Task<PecaResponse> UpdateEstoqueAsync(Guid id, AtualizarEstoqueRequest updateEstoqueDto);
    Task<IEnumerable<PecaResponse>> GetByEstoqueBaixoAsync(int limiteEstoque);
}