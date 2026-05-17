using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Common;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.UseCases.Servico.AtivarServico;
using OficinaMecanica.Application.UseCases.Servico.AtualizarServico;
using OficinaMecanica.Application.UseCases.Servico.ConsultarServico;
using OficinaMecanica.Application.UseCases.Servico.CriarServico;
using OficinaMecanica.Application.UseCases.Servico.DesativarServico;
using OficinaMecanica.Application.UseCases.Servico.ListarServicos;
using OficinaMecanica.Application.UseCases.Servico.ListarServicosAtivos;
using OficinaMecanica.Application.UseCases.Servico.ListarServicosPorNome;
using OficinaMecanica.Application.UseCases.Servico.RemoverServico;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class ServicosController : ControllerBase
{
    private readonly ICriarServicoUseCase _criar;
    private readonly IAtualizarServicoUseCase _atualizar;
    private readonly IConsultarServicoUseCase _consultar;
    private readonly IListarServicosUseCase _listar;
    private readonly IListarServicosAtivosUseCase _listarAtivos;
    private readonly IListarServicosPorNomeUseCase _listarPorNome;
    private readonly IRemoverServicoUseCase _remover;
    private readonly IAtivarServicoUseCase _ativar;
    private readonly IDesativarServicoUseCase _desativar;

    public ServicosController(
        ICriarServicoUseCase criar,
        IAtualizarServicoUseCase atualizar,
        IConsultarServicoUseCase consultar,
        IListarServicosUseCase listar,
        IListarServicosAtivosUseCase listarAtivos,
        IListarServicosPorNomeUseCase listarPorNome,
        IRemoverServicoUseCase remover,
        IAtivarServicoUseCase ativar,
        IDesativarServicoUseCase desativar)
    {
        _criar = criar;
        _atualizar = atualizar;
        _consultar = consultar;
        _listar = listar;
        _listarAtivos = listarAtivos;
        _listarPorNome = listarPorNome;
        _remover = remover;
        _ativar = ativar;
        _desativar = desativar;
    }

    /// <summary>
    /// Lista todos os serviços do catálogo (ativos e inativos)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServicoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listar.ExecutarAsync(Unit.Value);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Lista apenas os serviços ativos disponíveis para inclusão em ordens de serviço
    /// </summary>
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(IEnumerable<ServicoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAtivos()
    {
        var result = await _listarAtivos.ExecutarAsync(Unit.Value);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Obtém um serviço por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _consultar.ExecutarAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Busca serviços pelo nome (busca parcial)
    /// </summary>
    [HttpGet("nome/{nome}")]
    [ProducesResponseType(typeof(IEnumerable<ServicoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNome(string nome)
    {
        var result = await _listarPorNome.ExecutarAsync(nome);
        if (!result.IsSuccess)
            return this.MapError(result);

        var lista = result.Value;
        if (lista == null || !lista.Any())
            return NotFound();

        return Ok(lista);
    }

    /// <summary>
    /// Cadastra um novo serviço no catálogo da oficina
    /// </summary>
    /// <remarks>
    /// O `valor` deve ser maior que zero. O serviço é criado como ativo por padrão e fica
    /// disponível imediatamente para ser adicionado a ordens de serviço.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CriarServicoRequest request)
    {
        var result = await _criar.ExecutarAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : this.MapError(result);
    }

    /// <summary>
    /// Atualiza os dados de um serviço existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarServicoRequest dto)
    {
        var result = await _atualizar.ExecutarAsync(
            new AtualizarServicoUseCaseRequest(id, dto.Nome, dto.Descricao, dto.Valor));
        return result.IsSuccess ? Ok(result.Value) : this.MapError(result);
    }

    /// <summary>
    /// Remove permanentemente um serviço do catálogo
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
    /// Reativa um serviço previamente desativado, tornando-o disponível para novas OS
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
    /// Desativa um serviço sem removê-lo, impedindo sua inclusão em novas ordens de serviço
    /// </summary>
    [HttpPatch("{id:guid}/desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id)
    {
        var result = await _desativar.ExecutarAsync(id);
        return result.IsSuccess ? NoContent() : this.MapError(result);
    }
}
