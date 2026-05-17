using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<ClienteResponse?> GetByIdAsync(Guid id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null) return null;

        return new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Documento = cliente.Documento,
            Telefone = cliente.Telefone,
            Email = cliente.Email,
            Ativo = cliente.Ativo,
            CriadoEm = cliente.CriadoEm
        };
    }

    public async Task<ClienteResponse?> GetByDocumentoAsync(string documento)
    {
        var cliente = await _clienteRepository.GetByDocumentoAsync(documento);
        if (cliente == null) return null;

        return new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Documento = cliente.Documento,
            Telefone = cliente.Telefone,
            Email = cliente.Email,
            Ativo = cliente.Ativo,
            CriadoEm = cliente.CriadoEm
        };
    }

    public async Task<IEnumerable<ClienteResponse>> GetAllAsync()
    {
        var clientes = await _clienteRepository.GetAllAsync();
        return clientes.Select(cliente => new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Documento = cliente.Documento,
            Telefone = cliente.Telefone,
            Email = cliente.Email,
            Ativo = cliente.Ativo,
            CriadoEm = cliente.CriadoEm
        });
    }

    public async Task<ClienteResponse> CreateAsync(CriarClienteRequest createDto)
    {
        if (await _clienteRepository.ExistsByDocumentoAsync(createDto.Documento))
            throw new InvalidOperationException("Cliente com este documento já cadastrado");

        var cliente = new Cliente(createDto.Nome, createDto.Documento, createDto.Telefone, createDto.Email);
        var created = await _clienteRepository.AddAsync(cliente);

        return new ClienteResponse
        {
            Id = created.Id,
            Nome = created.Nome,
            Documento = created.Documento,
            Telefone = created.Telefone,
            Email = created.Email,
            Ativo = created.Ativo,
            CriadoEm = created.CriadoEm
        };
    }

    public async Task<ClienteResponse> UpdateAsync(Guid id, AtualizarClienteRequest updateDto)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
            throw new KeyNotFoundException("Cliente não encontrado");

        cliente.Atualizar(updateDto.Nome, updateDto.Telefone, updateDto.Email);
        await _clienteRepository.UpdateAsync(cliente);

        return new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Documento = cliente.Documento,
            Telefone = cliente.Telefone,
            Email = cliente.Email,
            Ativo = cliente.Ativo,
            CriadoEm = cliente.CriadoEm
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        await _clienteRepository.DeleteAsync(id);
    }

    public async Task AtivarAsync(Guid id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
            throw new KeyNotFoundException("Cliente não encontrado");

        cliente.Ativar();
        await _clienteRepository.UpdateAsync(cliente);
    }

    public async Task DesativarAsync(Guid id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
            throw new KeyNotFoundException("Cliente não encontrado");

        cliente.Desativar();
        await _clienteRepository.UpdateAsync(cliente);
    }
}
