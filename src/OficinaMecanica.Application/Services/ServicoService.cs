using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services;

public class ServicoService : IServicoService
{
    private readonly IServicoRepository _servicoRepository;

    public ServicoService(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public async Task<ServicoResponse?> GetByIdAsync(Guid id)
    {
        var servico = await _servicoRepository.GetByIdAsync(id);
        if (servico == null) return null;

        return MapToDto(servico);
    }

    public async Task<IEnumerable<ServicoResponse>> GetByNomeAsync(string nome)
    {
        var servicos = await _servicoRepository.GetByNomeAsync(nome);
        return servicos.Select(MapToDto);
    }

    public async Task<IEnumerable<ServicoResponse>> GetAllAsync()
    {
        var servicos = await _servicoRepository.GetAllAsync();
        return servicos.Select(MapToDto);
    }

    public async Task<IEnumerable<ServicoResponse>> GetAtivosAsync()
    {
        var servicos = await _servicoRepository.GetAtivosAsync();
        return servicos.Select(MapToDto);
    }

    public async Task<ServicoResponse> CreateAsync(CriarServicoRequest createDto)
    {
        if (await _servicoRepository.ExistsByNomeAsync(createDto.Nome))
            throw new InvalidOperationException("Serviço com este nome já cadastrado");

        var servico = new Servico(createDto.Nome, createDto.Descricao, createDto.Valor);
        var created = await _servicoRepository.AddAsync(servico);

        return MapToDto(created);
    }

    public async Task<ServicoResponse> UpdateAsync(Guid id, AtualizarServicoRequest updateDto)
    {
        var servico = await _servicoRepository.GetByIdAsync(id);
        if (servico == null)
            throw new KeyNotFoundException("Serviço não encontrado");

        servico.Atualizar(updateDto.Nome, updateDto.Descricao, updateDto.Valor);
        await _servicoRepository.UpdateAsync(servico);

        return MapToDto(servico);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _servicoRepository.DeleteAsync(id);
    }

    public async Task AtivarAsync(Guid id)
    {
        var servico = await _servicoRepository.GetByIdAsync(id);
        if (servico == null)
            throw new KeyNotFoundException("Serviço não encontrado");

        servico.Ativar();
        await _servicoRepository.UpdateAsync(servico);
    }

    public async Task DesativarAsync(Guid id)
    {
        var servico = await _servicoRepository.GetByIdAsync(id);
        if (servico == null)
            throw new KeyNotFoundException("Serviço não encontrado");

        servico.Desativar();
        await _servicoRepository.UpdateAsync(servico);
    }

    private static ServicoResponse MapToDto(Servico servico)
    {
        return new ServicoResponse
        {
            Id = servico.Id,
            Nome = servico.Nome,
            Descricao = servico.Descricao,
            Valor = servico.Valor,
            CriadoEm = servico.CriadoEm,
            Ativo = servico.Ativo
        };
    }
}