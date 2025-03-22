using System;
using System.Threading.Tasks;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClienteCadastro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var cliente = await _clienteService.GetByIdAsync(id);
                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _clienteService.GetPagedAsync(pageNumber, pageSize);
            return Ok(new
            {
                Data = result.Data,
                Total = result.Total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(result.Total / (double)pageSize)
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _clienteService.SearchAsync(searchTerm, pageNumber, pageSize);
            return Ok(new
            {
                Data = result.Data,
                Total = result.Total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(result.Total / (double)pageSize)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClienteDTO clienteDto)
        {
            try
            {
                var id = await _clienteService.CreateAsync(clienteDto);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ClienteDTO clienteDto)
        {
            if (id != clienteDto.Id)
                return BadRequest("O ID na URL não corresponde ao ID no corpo da requisição");

            try
            {
                var result = await _clienteService.UpdateAsync(clienteDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _clienteService.DeleteAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
