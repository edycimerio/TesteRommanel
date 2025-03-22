using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Queries.Cliente
{
    public class GetClientesPagedQueryHandler : IRequestHandler<GetClientesPagedQuery, (IEnumerable<ClienteDTO> Data, int Total)>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;

        public GetClientesPagedQueryHandler(
            IClienteRepository clienteRepository,
            IMapper mapper)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<ClienteDTO> Data, int Total)> Handle(GetClientesPagedQuery request, CancellationToken cancellationToken)
        {
            var result = await _clienteRepository.GetPagedAsync(request.PageNumber, request.PageSize);
            
            var clientesDto = _mapper.Map<IEnumerable<ClienteDTO>>(result.Data);
            
            return (clientesDto, result.Total);
        }
    }
}
