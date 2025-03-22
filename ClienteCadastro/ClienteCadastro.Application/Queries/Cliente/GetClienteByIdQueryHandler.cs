using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Queries.Cliente
{
    public class GetClienteByIdQueryHandler : IRequestHandler<GetClienteByIdQuery, ClienteDTO>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;

        public GetClienteByIdQueryHandler(
            IClienteRepository clienteRepository,
            IMapper mapper)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
        }

        public async Task<ClienteDTO> Handle(GetClienteByIdQuery request, CancellationToken cancellationToken)
        {
            var cliente = await _clienteRepository.GetClienteComEnderecosAsync(request.Id);
            if (cliente == null)
                throw new InvalidOperationException($"Cliente com ID {request.Id} não encontrado");

            return _mapper.Map<ClienteDTO>(cliente);
        }
    }
}
