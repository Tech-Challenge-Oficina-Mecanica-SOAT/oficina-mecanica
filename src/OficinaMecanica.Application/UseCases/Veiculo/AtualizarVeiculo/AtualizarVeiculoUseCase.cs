using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Domain.Interfaces;

namespace OficinaMecanica.Application.UseCases.Veiculo.AtualizarVeiculo;

public class AtualizarVeiculoUseCase : IAtualizarVeiculoUseCase
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly VeiculoMapper _mapper;

    public AtualizarVeiculoUseCase(IVeiculoRepository veiculoRepository, IClienteRepository clienteRepository, VeiculoMapper mapper)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
        _mapper = mapper;
    }

    public async Task<Result<VeiculoResponse>> ExecutarAsync(AtualizarVeiculoUseCaseRequest request)
    {
        var veiculo = await _veiculoRepository.GetByIdAsync(request.Id);
        if (veiculo is null)
            return Result<VeiculoResponse>.NotFound($"Veículo com ID {request.Id} não encontrado.");

        if (request.ClienteId.HasValue && request.ClienteId.Value != Guid.Empty)
        {
            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId.Value);
            if (cliente is null)
                return Result<VeiculoResponse>.NotFound($"Cliente com ID {request.ClienteId} não encontrado.");
        }

        if (await _veiculoRepository.ExistsByPlacaForOtherVeiculoAsync(request.Placa, request.Id))
            return Result<VeiculoResponse>.Conflict($"Veículo com placa {request.Placa} já cadastrado em outro veículo.");

        try
        {
            veiculo.Atualizar(request.ClienteId, request.Placa, request.Marca, request.Modelo, request.Ano);
            var updated = await _veiculoRepository.UpdateAsync(veiculo);
            return Result<VeiculoResponse>.Success(_mapper.MapToResponse(updated));
        }
        catch (ArgumentException ex)
        {
            return Result<VeiculoResponse>.Validation(ex.Message);
        }
    }
}
