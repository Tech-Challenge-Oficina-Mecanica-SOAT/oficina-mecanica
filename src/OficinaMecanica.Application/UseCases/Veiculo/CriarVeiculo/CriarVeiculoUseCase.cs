using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Application.UseCases.Veiculo.CriarVeiculo;

public class CriarVeiculoUseCase : ICriarVeiculoUseCase
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly VeiculoMapper _mapper;

    public CriarVeiculoUseCase(IVeiculoRepository veiculoRepository, IClienteRepository clienteRepository, VeiculoMapper mapper)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
        _mapper = mapper;
    }

    public async Task<Result<VeiculoResponse>> ExecutarAsync(CriarVeiculoRequest request)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
        if (cliente is null)
            return Result<VeiculoResponse>.NotFound($"Cliente com ID {request.ClienteId} não encontrado.");

        Placa placa;
        try
        {
            placa = new Placa(request.Placa);
        }
        catch (ArgumentException ex)
        {
            return Result<VeiculoResponse>.Validation(ex.Message);
        }

        if (await _veiculoRepository.ExistsByPlacaAsync(placa.Valor))
            return Result<VeiculoResponse>.Conflict($"Veículo com placa {placa.Valor} já cadastrado.");

        try
        {
            var veiculo = new Domain.Entities.Veiculo(request.ClienteId, placa, request.Marca, request.Modelo, request.Ano);
            var criado = await _veiculoRepository.AddAsync(veiculo);
            return Result<VeiculoResponse>.Success(_mapper.MapToResponse(criado));
        }
        catch (ArgumentException ex)
        {
            return Result<VeiculoResponse>.Validation(ex.Message);
        }
    }
}
