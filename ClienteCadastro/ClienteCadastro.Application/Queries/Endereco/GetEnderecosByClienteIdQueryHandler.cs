using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Queries.Endereco
{
    public class GetEnderecosByClienteIdQueryHandler : IRequestHandler<GetEnderecosByClienteIdQuery, IEnumerable<EnderecoDTO>>
    {
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IMapper _mapper;

        public GetEnderecosByClienteIdQueryHandler(IEnderecoRepository enderecoRepository, IMapper mapper)
        {
            _enderecoRepository = enderecoRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EnderecoDTO>> Handle(GetEnderecosByClienteIdQuery request, CancellationToken cancellationToken)
        {
            var enderecos = await _enderecoRepository.GetByClienteIdAsync(request.ClienteId);
            return _mapper.Map<IEnumerable<EnderecoDTO>>(enderecos);
        }
    }
}
