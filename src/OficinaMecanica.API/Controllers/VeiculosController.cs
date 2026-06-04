using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.UseCases.Veiculo.AtualizarVeiculo;
using OficinaMecanica.Application.UseCases.Veiculo.ConsultarVeiculo;
using OficinaMecanica.Application.UseCases.Veiculo.ConsultarVeiculoPorPlaca;
using OficinaMecanica.Application.UseCases.Veiculo.CriarVeiculo;
using OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculos;
using OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculosPorCliente;
using OficinaMecanica.Application.UseCases.Veiculo.RemoverVeiculo;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class VeiculosController : ControllerBase
{
    private readonly ICriarVeiculoUseCase _criar;
    private readonly IAtualizarVeiculoUseCase _atualizar;
    private readonly IConsultarVeiculoUseCase _consultar;
    private readonly IConsultarVeiculoPorPlacaUseCase _consultarPorPlaca;
    private readonly IListarVeiculosUseCase _listar;
    private readonly IListarVeiculosPorClienteUseCase _listarPorCliente;
    private readonly IRemoverVeiculoUseCase _remover;

    public VeiculosController(
        ICriarVeiculoUseCase criar,
        IAtualizarVeiculoUseCase atualizar,
        IConsultarVeiculoUseCase consultar,
        IConsultarVeiculoPorPlacaUseCase consultarPorPlaca,
        IListarVeiculosUseCase listar,
        IListarVeiculosPorClienteUseCase listarPorCliente,
        IRemoverVeiculoUseCase remover)
    {
        _criar = criar;
        _atualizar = atualizar;
        _consultar = consultar;
        _consultarPorPlaca = consultarPorPlaca;
        _listar = listar;
        _listarPorCliente = listarPorCliente;
        _remover = remover;
    }

    /// <summary>
    /// Lista todos os veículos cadastrados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VeiculoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listar.ExecutarAsync(Unit.Value);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Obtém um veículo por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _consultar.ExecutarAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Busca um veículo pela placa
    /// </summary>
    /// <remarks>
    /// A placa deve estar no formato Mercosul (ex.: `ABC1D23`) ou no formato antigo (ex.: `ABC1234`).
    /// </remarks>
    [HttpGet("placa/{placa}")]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPlaca(string placa)
    {
        var result = await _consultarPorPlaca.ExecutarAsync(placa);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Lista todos os veículos de um cliente
    /// </summary>
    [HttpGet("cliente/{clienteId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<VeiculoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClienteId(Guid clienteId)
    {
        var result = await _listarPorCliente.ExecutarAsync(clienteId);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Cadastra um novo veículo associado a um cliente existente
    /// </summary>
    /// <remarks>
    /// O `clienteId` deve corresponder a um cliente ativo. A placa deve ser única no sistema.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CriarVeiculoRequest request)
    {
        var result = await _criar.ExecutarAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : this.MapError(result);
    }

    /// <summary>
    /// Atualiza os dados de um veículo existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarVeiculoRequest dto)
    {
        var result = await _atualizar.ExecutarAsync(
            new AtualizarVeiculoUseCaseRequest(id, dto.ClienteId, dto.Placa, dto.Marca, dto.Modelo, dto.Ano));
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Remove permanentemente um veículo
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _remover.ExecutarAsync(id);
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }
}
