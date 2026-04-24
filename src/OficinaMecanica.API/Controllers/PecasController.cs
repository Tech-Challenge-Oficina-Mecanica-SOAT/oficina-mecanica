using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;

namespace OficinaMecanica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PecasController : ControllerBase
    {
        private readonly IPecaService _pecaService;

        public PecasController(IPecaService pecaService)
        {
            _pecaService = pecaService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var pecas = await _pecaService.GetAllAsync();
            return Ok(pecas);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var peca = await _pecaService.GetByIdAsync(id);
            if (peca == null)
                return NotFound(new { message = $"Peça com ID {id} não encontrada" });
            
            return Ok(peca);
        }

        [HttpGet("codigo/{codigo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var todasPecas = await _pecaService.GetAllAsync();
            var peca = todasPecas.FirstOrDefault(p => p.Codigo == codigo);
            
            if (peca == null)
                return NotFound(new { message = $"Peça com código '{codigo}' não encontrada" });
            
            return Ok(peca);
        }

        [HttpGet("estoque-baixo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEstoqueBaixo([FromQuery] int limite = 10)
        {
            var pecas = await _pecaService.GetByEstoqueBaixoAsync(limite);
            return Ok(pecas);
        }

        [HttpGet("{id}/estoque")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEstoque(Guid id)
        {
            var existe = await _pecaService.GetByIdAsync(id);
            if (existe == null)
                return NotFound(new { message = $"Peça com ID {id} não encontrada" });
            
            var estoque = await _pecaService.GetEstoqueAsync(id);
            return Ok(new { pecaId = id, estoque });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePecaDto createDto)
        {
            try
            {
                var peca = await _pecaService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = peca.Id }, peca);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePecaDto updateDto)
        {
            try
            {
                var peca = await _pecaService.UpdateAsync(id, updateDto);
                if (peca == null)
                    return NotFound(new { message = $"Peça com ID {id} não encontrada" });
                
                return Ok(peca);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/estoque")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEstoque(Guid id, [FromBody] UpdateEstoqueDto updateEstoqueDto)
        {
            try
            {
                var existe = await _pecaService.GetByIdAsync(id);
                if (existe == null)
                    return NotFound(new { message = $"Peça com ID {id} não encontrada" });
                
                await _pecaService.UpdateEstoqueAsync(id, updateEstoqueDto);
                var novoEstoque = await _pecaService.GetEstoqueAsync(id);

                return Ok(new {
                    success = true,
                    message = $"Estoque {(updateEstoqueDto.TipoOperacao == "incrementar" ? "incrementado" : "decrementado")} com sucesso",
                    novoEstoque
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existe = await _pecaService.GetByIdAsync(id);
            if (existe == null)
                return NotFound(new { message = $"Peça com ID {id} não encontrada" });
            
            await _pecaService.DeleteAsync(id);
            return NoContent();
        }
    }
}
