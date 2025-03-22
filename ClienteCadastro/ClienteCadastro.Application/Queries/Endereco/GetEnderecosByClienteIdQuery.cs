using System;
using System.Collections.Generic;
using ClienteCadastro.Application.DTOs;
using MediatR;

namespace ClienteCadastro.Application.Queries.Endereco
{
    public class GetEnderecosByClienteIdQuery : IRequest<IEnumerable<EnderecoDTO>>
    {
        public Guid ClienteId { get; set; }
    }
}
