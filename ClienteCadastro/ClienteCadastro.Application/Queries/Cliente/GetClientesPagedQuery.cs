using ClienteCadastro.Application.DTOs;
using MediatR;
using System.Collections.Generic;

namespace ClienteCadastro.Application.Queries.Cliente
{
    public class GetClientesPagedQuery : IRequest<(IEnumerable<ClienteDTO> Data, int Total)>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
