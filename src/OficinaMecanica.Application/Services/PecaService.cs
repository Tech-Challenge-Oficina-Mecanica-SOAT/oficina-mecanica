using OficinaMecanica.Application.DTOs;
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

    public async Task<PecaDto?> GetByIdAsync(Guid id)
    {
        var peca = await _pecaRepository.GetByIdAsync(id);
        return peca == null ? null : MapToDto(peca);
    }

    public async Task<IEnumerable<PecaDto>> GetAllAsync()
    {
        var pecas = await _pecaRepository.GetAllAsync();
        return pecas.Select(MapToDto);
    }

    public async Task<IEnumerable<PecaDto>> GetByNomeAsync(string nome)
    {
        var pecas = await _pecaRepository.GetByNomeAsync(nome);
        return pecas.Select(MapToDto);
    }

    public async Task<IEnumerable<PecaDto>> GetByEstoqueBaixoAsync(int limiteEstoque)
    {
        var pecas = await _pecaRepository.GetByEstoqueBaixoAsync(limiteEstoque);
        return pecas.Select(MapToDto);
    }

    public async Task<PecaDto> CreateAsync(CreatePecaDto createDto)
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

    public async Task<PecaDto?> UpdateAsync(Guid id, UpdatePecaDto updateDto)
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

    public async Task<PecaDto> UpdateEstoqueAsync(Guid id, UpdateEstoqueDto updateEstoqueDto)
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

    private static PecaDto MapToDto(PecaInsumo peca)
    {
        return new PecaDto
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
