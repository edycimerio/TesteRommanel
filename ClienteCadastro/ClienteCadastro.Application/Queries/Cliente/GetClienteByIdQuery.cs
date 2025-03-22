using System;
using ClienteCadastro.Application.DTOs;
using MediatR;

namespace ClienteCadastro.Application.Queries.Cliente
{
    public class GetClienteByIdQuery : IRequest<ClienteDTO>
    {
        public Guid Id { get; set; }
    }
}
