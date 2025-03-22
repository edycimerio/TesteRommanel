using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteCadastro.Application.Commands.Cliente.Delete;
using ClienteCadastro.Application.Commands.Cliente.Insert;
using ClienteCadastro.Application.Commands.Cliente.Update;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Application.Interfaces;
using ClienteCadastro.Application.Queries.Cliente;
using MediatR;

namespace ClienteCadastro.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IMediator _mediator;

        public ClienteService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Guid> CreateAsync(ClienteDTO clienteDto)
        {
            var command = new CreateClienteCommand
            {
                TipoPessoa = clienteDto.TipoPessoa,
                Nome = clienteDto.Nome,
                Documento = clienteDto.Documento,
                IE = clienteDto.IE,
                IsIsentoIE = clienteDto.IsIsentoIE,
                DataNascimento = clienteDto.DataNascimento,
                Telefone = clienteDto.Telefone,
                Email = clienteDto.Email,
                Enderecos = clienteDto.Enderecos
            };

            return await _mediator.Send(command);
        }

        public async Task<bool> UpdateAsync(ClienteDTO clienteDto)
        {
            var command = new UpdateClienteCommand
            {
                Id = clienteDto.Id,
                TipoPessoa = clienteDto.TipoPessoa,
                Nome = clienteDto.Nome,
                Documento = clienteDto.Documento,
                IE = clienteDto.IE,
                IsIsentoIE = clienteDto.IsIsentoIE,
                DataNascimento = clienteDto.DataNascimento,
                Telefone = clienteDto.Telefone,
                Email = clienteDto.Email,
                Ativo = clienteDto.Ativo,
                Enderecos = clienteDto.Enderecos
            };

            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var command = new DeleteClienteCommand { Id = id };
            return await _mediator.Send(command);
        }

        public async Task<ClienteDTO> GetByIdAsync(Guid id)
        {
            var query = new GetClienteByIdQuery { Id = id };
            return await _mediator.Send(query);
        }

        public async Task<(IEnumerable<ClienteDTO> Data, int Total)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = new GetClientesPagedQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await _mediator.Send(query);
        }

        public async Task<(IEnumerable<ClienteDTO> Data, int Total)> SearchAsync(string searchTerm, int pageNumber, int pageSize)
        {
            var query = new GetClientesSearchQuery
            {
                SearchTerm = searchTerm,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await _mediator.Send(query);
        }
    }
}
