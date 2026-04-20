using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services
{
    public class PecaService : IPecaService
    {
        private readonly IPecaRepository _pecaRepository;

        public PecaService(IPecaRepository pecaRepository)
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

        public async Task<IEnumerable<PecaDto>> GetByEstoqueBaixoAsync(int limiteEstoque)
        {
            var pecas = await _pecaRepository.GetByEstoqueBaixoAsync(limiteEstoque);
            return pecas.Select(MapToDto);
        }

        public async Task<PecaDto> CreateAsync(CreatePecaDto createDto)
        {
            var existeCodigo = await _pecaRepository.ExistsByCodigoAsync(createDto.Codigo);
            if (existeCodigo)
            {
                throw new InvalidOperationException($"Já existe uma peça com o código '{createDto.Codigo}'");
            }

            if (createDto.PrecoUnitario <= 0)
            {
                throw new InvalidOperationException("O preço unitário deve ser maior que zero");
            }

            if (createDto.Estoque < 0)
            {
                throw new InvalidOperationException("O estoque não pode ser negativo");
            }

            var peca = new Peca
            {
                Nome = createDto.Nome,
                Codigo = createDto.Codigo,
                PrecoUnitario = createDto.PrecoUnitario,
                Estoque = createDto.Estoque,
                Descricao = createDto.Descricao
            };

            var created = await _pecaRepository.AddAsync(peca);
            return MapToDto(created);
        }

        public async Task<PecaDto?> UpdateAsync(Guid id, UpdatePecaDto updateDto)
        {
            var peca = await _pecaRepository.GetByIdAsync(id);
            if (peca == null) return null;

            if (updateDto.PrecoUnitario <= 0)
            {
                throw new InvalidOperationException("O preço unitário deve ser maior que zero");
            }

            if (updateDto.Estoque < 0)
            {
                throw new InvalidOperationException("O estoque não pode ser negativo");
            }

            peca.Nome = updateDto.Nome;
            peca.PrecoUnitario = updateDto.PrecoUnitario;
            peca.Estoque = updateDto.Estoque;
            peca.Descricao = updateDto.Descricao;

            var updated = await _pecaRepository.UpdateAsync(peca);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _pecaRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateEstoqueAsync(Guid id, UpdateEstoqueDto updateEstoqueDto)
        {
            if (updateEstoqueDto.Quantidade <= 0)
            {
                throw new InvalidOperationException("A quantidade deve ser maior que zero");
            }

            if (updateEstoqueDto.TipoOperacao.ToLower() == "incrementar")
            {
                return await _pecaRepository.IncrementarEstoqueAsync(id, updateEstoqueDto.Quantidade);
            }
            else if (updateEstoqueDto.TipoOperacao.ToLower() == "decrementar")
            {
                var estoqueAtual = await _pecaRepository.GetEstoqueAsync(id);
                if (estoqueAtual < updateEstoqueDto.Quantidade)
                {
                    throw new InvalidOperationException($"Estoque insuficiente. Disponível: {estoqueAtual}");
                }
                return await _pecaRepository.DecrementarEstoqueAsync(id, updateEstoqueDto.Quantidade);
            }
            else
            {
                throw new InvalidOperationException("Tipo de operação inválido. Use 'incrementar' ou 'decrementar'");
            }
        }

        public async Task<int> GetEstoqueAsync(Guid id)
        {
            return await _pecaRepository.GetEstoqueAsync(id);
        }

        private static PecaDto MapToDto(Peca peca)
        {
            return new PecaDto
            {
                Id = peca.Id,
                Nome = peca.Nome,
                Codigo = peca.Codigo,
                PrecoUnitario = peca.PrecoUnitario,
                Estoque = peca.Estoque,
                Descricao = peca.Descricao,
                CriadoEm = peca.CriadoEm,
                AtualizadoEm = peca.AtualizadoEm
            };
        }
    }
}
