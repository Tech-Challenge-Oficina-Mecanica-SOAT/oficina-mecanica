using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;

namespace OficinaMecanica.Application.Interfaces;

public interface IServicoService
{
    Task<ServicoResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServicoResponse>> GetByNomeAsync(string nome);
    Task<IEnumerable<ServicoResponse>> GetAllAsync();
    Task<IEnumerable<ServicoResponse>> GetAtivosAsync();
    Task<ServicoResponse> CreateAsync(CriarServicoRequest createDto);
    Task<ServicoResponse> UpdateAsync(Guid id, AtualizarServicoRequest updateDto);
    Task DeleteAsync(Guid id);
    Task AtivarAsync(Guid id);
    Task DesativarAsync(Guid id);
}
