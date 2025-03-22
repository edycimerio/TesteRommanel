using System;
using MediatR;

namespace ClienteCadastro.Application.Commands.Cliente.Delete
{
    public class DeleteClienteCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
