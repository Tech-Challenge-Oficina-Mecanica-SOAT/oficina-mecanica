using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.UseCases.Peca.AtualizarEstoque;
using OficinaMecanica.Application.UseCases.Peca.AtualizarPeca;
using OficinaMecanica.Application.UseCases.Peca.ConsultarPeca;
using OficinaMecanica.Application.UseCases.Peca.CriarPeca;
using OficinaMecanica.Application.UseCases.Peca.ListarPecas;
using OficinaMecanica.Application.UseCases.Peca.ListarPecasEstoqueBaixo;
using OficinaMecanica.Application.UseCases.Peca.ObterEstoque;
using OficinaMecanica.Application.UseCases.Peca.RemoverPeca;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class PecasController : ControllerBase
{
    private readonly ICriarPecaUseCase _criar;
    private readonly IAtualizarPecaUseCase _atualizar;
    private readonly IAtualizarEstoqueUseCase _atualizarEstoque;
    private readonly IConsultarPecaUseCase _consultar;
    private readonly IListarPecasUseCase _listar;
    private readonly IListarPecasEstoqueBaixoUseCase _listarEstoqueBaixo;
    private readonly IObterEstoqueUseCase _obterEstoque;
    private readonly IRemoverPecaUseCase _remover;

    public PecasController(
        ICriarPecaUseCase criar,
        IAtualizarPecaUseCase atualizar,
        IAtualizarEstoqueUseCase atualizarEstoque,
        IConsultarPecaUseCase consultar,
        IListarPecasUseCase listar,
        IListarPecasEstoqueBaixoUseCase listarEstoqueBaixo,
        IObterEstoqueUseCase obterEstoque,
        IRemoverPecaUseCase remover)
    {
        _criar = criar;
        _atualizar = atualizar;
        _atualizarEstoque = atualizarEstoque;
        _consultar = consultar;
        _listar = listar;
        _listarEstoqueBaixo = listarEstoqueBaixo;
        _obterEstoque = obterEstoque;
        _remover = remover;
    }

    /// <summary>
    /// Lista todas as peças e insumos do estoque
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listar.ExecutarAsync(Unit.Value);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Obtém uma peça por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _consultar.ExecutarAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Busca uma peça pelo código interno (SKU)
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCodigo(string codigo)
    {
        var result = await _listar.ExecutarAsync(Unit.Value);
        if (!result.IsSuccess)
            return this.MapError(result);

        var peca = result.Value!.FirstOrDefault(p => p.Codigo == codigo);
        if (peca is null)
            return NotFound(new { message = $"Peça com código '{codigo}' não encontrada" });

        return Ok(peca);
    }

    /// <summary>
    /// Lista peças com estoque igual ou abaixo do limite informado
    /// </summary>
    /// <remarks>
    /// Use este endpoint para identificar itens que precisam de reposição.
    /// O parâmetro `limite` tem valor padrão `10` quando não informado.
    /// </remarks>
    [HttpGet("estoque-baixo")]
    [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstoqueBaixo([FromQuery] int limite = 10)
    {
        var result = await _listarEstoqueBaixo.ExecutarAsync(limite);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Retorna a quantidade atual em estoque de uma peça
    /// </summary>
    [HttpGet("{id:guid}/estoque")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEstoque(Guid id)
    {
        var result = await _obterEstoque.ExecutarAsync(id);
        return result.IsSuccess
            ? Ok(new { pecaId = id, estoque = result.Value })
            : this.MapError(result);
    }

    /// <summary>
    /// Cadastra uma nova peça ou insumo no catálogo
    /// </summary>
    /// <remarks>
    /// O `codigo` deve ser único. O estoque inicial pode ser zero; use `PATCH /{id}/estoque` para movimentar.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CriarPecaRequest request)
    {
        var result = await _criar.ExecutarAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : this.MapError(result);
    }

    /// <summary>
    /// Atualiza os dados cadastrais de uma peça (nome, código, preço)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarPecaRequest dto)
    {
        var result = await _atualizar.ExecutarAsync(
            new AtualizarPecaUseCaseRequest(id, dto.Nome, dto.Descricao, dto.PrecoUnitario, dto.Estoque));
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Movimenta o estoque de uma peça (entrada ou saída)
    /// </summary>
    /// <remarks>
    /// O campo **tipoOperacao** aceita: `incrementar` (entrada) ou `decrementar` (saída/uso em OS).
    /// Decrementar além do saldo disponível retorna `400 Bad Request`.
    /// </remarks>
    [HttpPatch("{id:guid}/estoque")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEstoque(Guid id, [FromBody] AtualizarEstoqueRequest dto)
    {
        var result = await _atualizarEstoque.ExecutarAsync(
            new AtualizarEstoqueUseCaseRequest(id, dto.Quantidade, dto.TipoOperacao));
        if (!result.IsSuccess)
            return this.MapError(result);

        return Ok(new
        {
            success = true,
            message = $"Estoque {(dto.TipoOperacao == "incrementar" ? "incrementado" : "decrementado")} com sucesso",
            novoEstoque = result.Value!.Estoque
        });
    }

    /// <summary>
    /// Remove permanentemente uma peça do catálogo
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
