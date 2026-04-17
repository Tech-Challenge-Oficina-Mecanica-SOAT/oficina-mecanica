using OficinaMecanica.Application.DTOs;
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

    public async Task<VeiculoDto?> GetByIdAsync(Guid id)
    {
        var veiculo = await _veiculoRepository.GetByIdAsync(id);
        if (veiculo == null) return null;
        
        var cliente = await _clienteRepository.GetByIdAsync(veiculo.ClienteId);
        
        return new VeiculoDto
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            ClienteNome = cliente?.Nome ?? string.Empty,
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano,
            CriadoEm = veiculo.CriadoEm
        };
    }

    public async Task<VeiculoDto?> GetByPlacaAsync(string placa)
    {
        var veiculo = await _veiculoRepository.GetByPlacaAsync(placa);
        if (veiculo == null) return null;
        
        var cliente = await _clienteRepository.GetByIdAsync(veiculo.ClienteId);
        
        return new VeiculoDto
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            ClienteNome = cliente?.Nome ?? string.Empty,
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano,
            CriadoEm = veiculo.CriadoEm
        };
    }

    public async Task<IEnumerable<VeiculoDto>> GetAllAsync()
    {
        var veiculos = await _veiculoRepository.GetAllAsync();
        var result = new List<VeiculoDto>();
        
        foreach (var veiculo in veiculos)
        {
            var cliente = await _clienteRepository.GetByIdAsync(veiculo.ClienteId);
            result.Add(new VeiculoDto
            {
                Id = veiculo.Id,
                ClienteId = veiculo.ClienteId,
                ClienteNome = cliente?.Nome ?? string.Empty,
                Placa = veiculo.Placa,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                Ano = veiculo.Ano,
                CriadoEm = veiculo.CriadoEm
            });
        }
        
        return result;
    }

    public async Task<IEnumerable<VeiculoDto>> GetByClienteIdAsync(Guid clienteId)
    {
        var veiculos = await _veiculoRepository.GetByClienteIdAsync(clienteId);
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        var clienteNome = cliente?.Nome ?? string.Empty;
        
        return veiculos.Select(veiculo => new VeiculoDto
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            ClienteNome = clienteNome,
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano,
            CriadoEm = veiculo.CriadoEm
        });
    }

    public async Task<VeiculoDto> CreateAsync(CreateVeiculoDto createDto)
    {
        // Verificar se o cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(createDto.ClienteId);
        if (cliente == null)
            throw new KeyNotFoundException("Cliente não encontrado");
            
        // Verificar se a placa já existe
        if (await _veiculoRepository.ExistsByPlacaAsync(createDto.Placa))
            throw new InvalidOperationException("Veículo com esta placa já cadastrado");
        
        var veiculo = new Veiculo(createDto.ClienteId, createDto.Placa, createDto.Marca, createDto.Modelo, createDto.Ano);
        var created = await _veiculoRepository.AddAsync(veiculo);
        
        return new VeiculoDto
        {
            Id = created.Id,
            ClienteId = created.ClienteId,
            ClienteNome = cliente.Nome,
            Placa = created.Placa,
            Marca = created.Marca,
            Modelo = created.Modelo,
            Ano = created.Ano,
            CriadoEm = created.CriadoEm
        };
    }

    public async Task<VeiculoDto> UpdateAsync(Guid id, UpdateVeiculoDto updateDto)
    {
        var veiculo = await _veiculoRepository.GetByIdAsync(id);
        if (veiculo == null)
            throw new KeyNotFoundException("Veículo não encontrado");
            
        // Verificar se a placa já existe para outro veículo
        if (await _veiculoRepository.ExistsByPlacaForOtherClienteAsync(updateDto.Placa, veiculo.ClienteId, id))
            throw new InvalidOperationException("Veículo com esta placa já cadastrado");
            
        veiculo.Atualizar(updateDto.Placa, updateDto.Marca, updateDto.Modelo, updateDto.Ano);
        await _veiculoRepository.UpdateAsync(veiculo);
        
        var cliente = await _clienteRepository.GetByIdAsync(veiculo.ClienteId);
        
        return new VeiculoDto
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            ClienteNome = cliente?.Nome ?? string.Empty,
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano,
            CriadoEm = veiculo.CriadoEm
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        await _veiculoRepository.DeleteAsync(id);
    }
}
