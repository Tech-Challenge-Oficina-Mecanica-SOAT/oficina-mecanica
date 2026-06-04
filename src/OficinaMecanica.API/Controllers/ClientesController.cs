using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.UseCases.Cliente.AtivarCliente;
using OficinaMecanica.Application.UseCases.Cliente.AtualizarCliente;
using OficinaMecanica.Application.UseCases.Cliente.ConsultarCliente;
using OficinaMecanica.Application.UseCases.Cliente.ConsultarClientePorDocumento;
using OficinaMecanica.Application.UseCases.Cliente.CriarCliente;
using OficinaMecanica.Application.UseCases.Cliente.DesativarCliente;
using OficinaMecanica.Application.UseCases.Cliente.ListarClientes;
using OficinaMecanica.Application.UseCases.Cliente.RemoverCliente;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class ClientesController : ControllerBase
{
    private readonly ICriarClienteUseCase _criar;
    private readonly IAtualizarClienteUseCase _atualizar;
    private readonly IConsultarClienteUseCase _consultar;
    private readonly IConsultarClientePorDocumentoUseCase _consultarPorDocumento;
    private readonly IListarClientesUseCase _listar;
    private readonly IRemoverClienteUseCase _remover;
    private readonly IAtivarClienteUseCase _ativar;
    private readonly IDesativarClienteUseCase _desativar;

    public ClientesController(
        ICriarClienteUseCase criar,
        IAtualizarClienteUseCase atualizar,
        IConsultarClienteUseCase consultar,
        IConsultarClientePorDocumentoUseCase consultarPorDocumento,
        IListarClientesUseCase listar,
        IRemoverClienteUseCase remover,
        IAtivarClienteUseCase ativar,
        IDesativarClienteUseCase desativar)
    {
        _criar = criar;
        _atualizar = atualizar;
        _consultar = consultar;
        _consultarPorDocumento = consultarPorDocumento;
        _listar = listar;
        _remover = remover;
        _ativar = ativar;
        _desativar = desativar;
    }

    /// <summary>
    /// Lista todos os clientes cadastrados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listar.ExecutarAsync(Unit.Value);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Obtém um cliente por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _consultar.ExecutarAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Busca um cliente pelo CPF ou CNPJ
    /// </summary>
    [HttpGet("documento/{documento}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDocumento(string documento)
    {
        var result = await _consultarPorDocumento.ExecutarAsync(documento);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Cadastra um novo cliente
    /// </summary>
    /// <remarks>
    /// O campo `documento` aceita CPF (11 dígitos) ou CNPJ (14 dígitos), somente números.
    /// Documentos duplicados retornam `400 Bad Request`.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CriarClienteRequest request)
    {
        var result = await _criar.ExecutarAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : this.MapError(result);
    }

    /// <summary>
    /// Atualiza os dados de contato de um cliente existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarClienteRequest dto)
    {
        var result = await _atualizar.ExecutarAsync(
            new AtualizarClienteUseCaseRequest(id, dto.Nome, dto.Telefone, dto.Email));
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Remove permanentemente um cliente
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _remover.ExecutarAsync(id);
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Reativa um cliente previamente desativado
    /// </summary>
    [HttpPatch("{id:guid}/ativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(Guid id)
    {
        var result = await _ativar.ExecutarAsync(id);
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }

    /// <summary>
    /// Desativa um cliente sem removê-lo do sistema
    /// </summary>
    /// <remarks>
    /// Clientes desativados não aparecem nas listagens padrão, mas seus dados e histórico de OS são preservados.
    /// </remarks>
    [HttpPatch("{id:guid}/desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id)
    {
        var result = await _desativar.ExecutarAsync(id);
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }
}
