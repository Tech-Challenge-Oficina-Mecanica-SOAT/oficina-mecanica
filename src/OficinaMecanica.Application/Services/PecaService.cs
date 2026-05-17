using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services;

public class PecaService : IPecaService
{
    private readonly IPecaInsumoRepository _pecaRepository;

    public PecaService(IPecaInsumoRepository pecaRepository)
    {
        _pecaRepository = pecaRepository;
    }

    public async Task<PecaResponse?> GetByIdAsync(Guid id)
    {
        var peca = await _pecaRepository.GetByIdAsync(id);
        return peca == null ? null : MapToDto(peca);
    }

    public async Task<IEnumerable<PecaResponse>> GetAllAsync()
    {
        var pecas = await _pecaRepository.GetAllAsync();
        return pecas.Select(MapToDto);
    }

    public async Task<IEnumerable<PecaResponse>> GetByNomeAsync(string nome)
    {
        var pecas = await _pecaRepository.GetByNomeAsync(nome);
        return pecas.Select(MapToDto);
    }

    public async Task<IEnumerable<PecaResponse>> GetByEstoqueBaixoAsync(int limiteEstoque)
    {
        var pecas = await _pecaRepository.GetByEstoqueBaixoAsync(limiteEstoque);
        return pecas.Select(MapToDto);
    }

    public async Task<PecaResponse> CreateAsync(CriarPecaRequest createDto)
    {
        var existeCodigo = await _pecaRepository.ExistsByCodigoAsync(createDto.Codigo);
        if (existeCodigo)
            throw new InvalidOperationException("Já existe uma peça com este código");

        var peca = new PecaInsumo(
            createDto.Nome,
            createDto.Codigo,
            createDto.Descricao,
            createDto.PrecoUnitario,
            createDto.Estoque
        );

        var created = await _pecaRepository.AddAsync(peca);
        return MapToDto(created);
    }

    public async Task<PecaResponse?> UpdateAsync(Guid id, AtualizarPecaRequest updateDto)
    {
        var peca = await _pecaRepository.GetByIdAsync(id);
        if (peca == null) return null;

        peca.Atualizar(updateDto.Nome, updateDto.Descricao, updateDto.PrecoUnitario, updateDto.Estoque);

        var updated = await _pecaRepository.UpdateAsync(peca);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _pecaRepository.DeleteAsync(id);
    }

    public async Task<int> GetEstoqueAsync(Guid id)
    {
        return await _pecaRepository.GetEstoqueAsync(id);
    }

    public async Task<PecaResponse> UpdateEstoqueAsync(Guid id, AtualizarEstoqueRequest updateEstoqueDto)
    {
        PecaInsumo peca;

        if (updateEstoqueDto.TipoOperacao == "incrementar")
        {
            peca = await _pecaRepository.IncrementarEstoqueAsync(id, updateEstoqueDto.Quantidade);
        }
        else if (updateEstoqueDto.TipoOperacao == "decrementar")
        {
            peca = await _pecaRepository.DecrementarEstoqueAsync(id, Math.Abs(updateEstoqueDto.Quantidade));
        }
        else
        {
            throw new ArgumentException($"tipoOperacao inválido: '{updateEstoqueDto.TipoOperacao}'. Use 'incrementar' ou 'decrementar'.");
        }

        return MapToDto(peca);
    }

    private static PecaResponse MapToDto(PecaInsumo peca)
    {
        return new PecaResponse
        {
            Id = peca.Id,
            Nome = peca.Nome,
            Codigo = peca.Codigo,
            Descricao = peca.Descricao,
            PrecoUnitario = peca.Preco,
            Estoque = peca.Quantidade,
            CriadoEm = peca.CriadoEm,
            Ativo = peca.Ativo
        };
    }
}
