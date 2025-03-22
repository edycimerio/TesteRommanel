using System;
using ClienteCadastro.Application.DTOs;
using MediatR;

namespace ClienteCadastro.Application.Queries.Endereco
{
    public class GetEnderecoByIdQuery : IRequest<EnderecoDTO>
    {
        public Guid Id { get; set; }
    }
}
