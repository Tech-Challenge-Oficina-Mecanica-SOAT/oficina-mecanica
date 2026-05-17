using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.Services;

public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;

    public VeiculoService(IVeiculoRepository veiculoRepository, IClienteRepository clienteRepository)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<VeiculoResponse?> GetByIdAsync(Guid id)
    {
        var veiculo = await _veiculoRepository.GetByIdAsync(id);
        if (veiculo == null) return null;

        return MapToDto(veiculo);
    }

    public async Task<VeiculoResponse?> GetByPlacaAsync(string placa)
    {
        var veiculo = await _veiculoRepository.GetByPlacaAsync(placa);
        if (veiculo == null) return null;

        return MapToDto(veiculo);
    }

    public async Task<IEnumerable<VeiculoResponse>> GetAllAsync()
    {
        var veiculos = await _veiculoRepository.GetAllAsync();
        return veiculos.Select(MapToDto);
    }

    public async Task<IEnumerable<VeiculoResponse>> GetByClienteIdAsync(Guid clienteId)
    {
        var veiculos = await _veiculoRepository.GetByClienteIdAsync(clienteId);
        return veiculos.Select(MapToDto);
    }

    public async Task<VeiculoResponse> CreateAsync(CriarVeiculoRequest createDto)
    {
        // Validar se o cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(createDto.ClienteId);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente com ID {createDto.ClienteId} não encontrado");

        // Validar se a placa já existe
        if (await _veiculoRepository.ExistsByPlacaAsync(createDto.Placa))
            throw new InvalidOperationException($"Veículo com placa {createDto.Placa} já cadastrado");

        var veiculo = new Veiculo(createDto.ClienteId, createDto.Placa, createDto.Marca, createDto.Modelo, createDto.Ano);
        var created = await _veiculoRepository.AddAsync(veiculo);

        return MapToDto(created);
    }

    public async Task<VeiculoResponse> UpdateAsync(Guid id, AtualizarVeiculoRequest updateDto)
    {
        var veiculo = await _veiculoRepository.GetByIdAsync(id);
        if (veiculo == null)
            throw new KeyNotFoundException($"Veículo com ID {id} não encontrado");

        // Validar se o cliente existe (se foi informado um novo cliente)
        if (updateDto.ClienteId.HasValue && updateDto.ClienteId.Value != Guid.Empty)
        {
            var cliente = await _clienteRepository.GetByIdAsync(updateDto.ClienteId.Value);
            if (cliente == null)
                throw new KeyNotFoundException($"Cliente com ID {updateDto.ClienteId} não encontrado");
        }

        // Validar se a placa já existe em outro veículo
        if (await _veiculoRepository.ExistsByPlacaForOtherVeiculoAsync(updateDto.Placa, id))
            throw new InvalidOperationException($"Veículo com placa {updateDto.Placa} já cadastrado em outro veículo");

        veiculo.Atualizar(
            updateDto.ClienteId,
            updateDto.Placa,
            updateDto.Marca,
            updateDto.Modelo,
            updateDto.Ano
        );

        var updated = await _veiculoRepository.UpdateAsync(veiculo);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _veiculoRepository.DeleteAsync(id);
    }

    private static VeiculoResponse MapToDto(Veiculo veiculo)
    {
        return new VeiculoResponse
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            ClienteNome = veiculo.Cliente?.Nome ?? string.Empty,
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano,
            CriadoEm = veiculo.CriadoEm
        };
    }
}
