using ClienteCadastro.Application.DTOs;
using MediatR;
using System.Collections.Generic;

namespace ClienteCadastro.Application.Queries.Cliente
{
    public class GetClientesSearchQuery : IRequest<(IEnumerable<ClienteDTO> Data, int Total)>
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
