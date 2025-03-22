using System;
using MediatR;

namespace ClienteCadastro.Application.Commands.Endereco.Delete
{
    public class DeleteEnderecoCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
