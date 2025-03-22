using System;
using System.Threading.Tasks;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Application.Queries.Endereco;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClienteCadastro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnderecosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EnderecosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var query = new GetEnderecoByIdQuery { Id = id };
                var endereco = await _mediator.Send(query);
                
                if (endereco == null)
                    return NotFound($"Endereço com ID {id} não encontrado");
                    
                return Ok(endereco);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> GetByClienteId(Guid clienteId)
        {
            try
            {
                var query = new GetEnderecosByClienteIdQuery { ClienteId = clienteId };
                var enderecos = await _mediator.Send(query);
                return Ok(enderecos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EnderecoDTO enderecoDto)
        {
            try
            {
                var command = new Application.Commands.Endereco.Insert.CreateEnderecoCommand
                {
                    ClienteId = enderecoDto.ClienteId,
                    CEP = enderecoDto.CEP,
                    Logradouro = enderecoDto.Logradouro,
                    Numero = enderecoDto.Numero,
                    Complemento = enderecoDto.Complemento,
                    Bairro = enderecoDto.Bairro,
                    Cidade = enderecoDto.Cidade,
                    Estado = enderecoDto.Estado
                };

                var id = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EnderecoDTO enderecoDto)
        {
            if (id != enderecoDto.Id)
                return BadRequest("O ID na URL não corresponde ao ID no corpo da requisição");

            try
            {
                var command = new Application.Commands.Endereco.Update.UpdateEnderecoCommand
                {
                    Id = enderecoDto.Id,
                    ClienteId = enderecoDto.ClienteId,
                    CEP = enderecoDto.CEP,
                    Logradouro = enderecoDto.Logradouro,
                    Numero = enderecoDto.Numero,
                    Complemento = enderecoDto.Complemento,
                    Bairro = enderecoDto.Bairro,
                    Cidade = enderecoDto.Cidade,
                    Estado = enderecoDto.Estado
                };

                var result = await _mediator.Send(command);
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
                var command = new Application.Commands.Endereco.Delete.DeleteEnderecoCommand { Id = id };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
