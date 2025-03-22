using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Queries.Endereco
{
    public class GetEnderecoByIdQueryHandler : IRequestHandler<GetEnderecoByIdQuery, EnderecoDTO>
    {
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IMapper _mapper;

        public GetEnderecoByIdQueryHandler(IEnderecoRepository enderecoRepository, IMapper mapper)
        {
            _enderecoRepository = enderecoRepository;
            _mapper = mapper;
        }

        public async Task<EnderecoDTO> Handle(GetEnderecoByIdQuery request, CancellationToken cancellationToken)
        {
            var endereco = await _enderecoRepository.GetByIdAsync(request.Id);
            
            if (endereco == null)
                return null;
                
            return _mapper.Map<EnderecoDTO>(endereco);
        }
    }
}
