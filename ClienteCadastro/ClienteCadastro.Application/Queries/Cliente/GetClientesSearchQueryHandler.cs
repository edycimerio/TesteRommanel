using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Queries.Cliente
{
    public class GetClientesSearchQueryHandler : IRequestHandler<GetClientesSearchQuery, (IEnumerable<ClienteDTO> Data, int Total)>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;

        public GetClientesSearchQueryHandler(
            IClienteRepository clienteRepository,
            IMapper mapper)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<ClienteDTO> Data, int Total)> Handle(GetClientesSearchQuery request, CancellationToken cancellationToken)
        {
            var result = await _clienteRepository.GetAllSearchAsync(request.SearchTerm, request.PageNumber, request.PageSize);
            
            var clientesDto = _mapper.Map<IEnumerable<ClienteDTO>>(result.Data);
            
            return (clientesDto, result.Total);
        }
    }
}
