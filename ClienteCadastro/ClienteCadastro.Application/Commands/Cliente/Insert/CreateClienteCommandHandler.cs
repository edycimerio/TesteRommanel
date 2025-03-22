using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Commands.Cliente.Insert
{
    public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, Guid>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEventStore _eventStore;

        public CreateClienteCommandHandler(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEventStore eventStore)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _eventStore = eventStore;
        }

        public async Task<Guid> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
        {
            // Verificar se já existe cliente com o mesmo documento ou email
            if (await _clienteRepository.DocumentoExisteAsync(request.Documento))
                throw new InvalidOperationException($"Já existe um cliente cadastrado com o documento {request.Documento}");

            if (await _clienteRepository.EmailExisteAsync(request.Email))
                throw new InvalidOperationException($"Já existe um cliente cadastrado com o email {request.Email}");

            // Validações específicas por tipo de pessoa
            if (request.TipoPessoa == 'F')
            {
                // Validar idade mínima (18 anos) para pessoa física
                var idadeMinima = 18;
                var idade = DateTime.Today.Year - request.DataNascimento.Year;
                if (request.DataNascimento.Date > DateTime.Today.AddYears(-idade)) idade--;
                
                if (idade < idadeMinima)
                    throw new InvalidOperationException($"A idade mínima para cadastro é de {idadeMinima} anos.");
            }
            else // TipoPessoa == 'J'
            {
                // Validar IE obrigatório para pessoa jurídica (a menos que seja isento)
                if (string.IsNullOrEmpty(request.IE) && !request.IsIsentoIE)
                    throw new InvalidOperationException("Para pessoa jurídica, é necessário informar a Inscrição Estadual ou marcar como Isento.");
            }

            // Criar o cliente
            Domain.Entities.Cliente cliente;
            
            if (request.TipoPessoa == 'F')
            {
                // Criar cliente pessoa física
                cliente = new Domain.Entities.Cliente(
                    request.Nome,
                    request.Documento,
                    request.DataNascimento,
                    request.Telefone,
                    request.Email
                );
            }
            else // TipoPessoa == 'J'
            {
                // Criar cliente pessoa jurídica
                cliente = new Domain.Entities.Cliente(
                    request.Nome,
                    request.Documento,
                    request.IE ?? string.Empty,
                    request.IsIsentoIE,
                    request.DataNascimento,
                    request.Telefone,
                    request.Email
                );
            }

            // Adicionar endereços
            foreach (var enderecoDto in request.Enderecos)
            {
                var endereco = new Domain.Entities.Endereco(
                    cliente.Id,
                    enderecoDto.CEP,
                    enderecoDto.Logradouro,
                    enderecoDto.Numero,
                    enderecoDto.Bairro,
                    enderecoDto.Cidade,
                    enderecoDto.Estado
                );
                cliente.AdicionarEndereco(endereco);
            }

            // Salvar o cliente
            await _clienteRepository.AddAsync(cliente);
            await _unitOfWork.CommitAsync();

            // Registrar evento
            await _eventStore.SaveEventAsync(
                new ClienteCriadoEvent(cliente.Id, cliente.Nome, cliente.Documento, cliente.Email),
                cliente.Id,
                "Cliente",
                1
            );

            return cliente.Id;
        }
    }

    public class ClienteCriadoEvent : Event
    {
        public Guid ClienteId { get; private set; }
        public string Nome { get; private set; }
        public string Documento { get; private set; }
        public string Email { get; private set; }

        public ClienteCriadoEvent(Guid clienteId, string nome, string documento, string email)
        {
            AggregateId = clienteId;
            ClienteId = clienteId;
            Nome = nome;
            Documento = documento;
            Email = email;
        }
    }
}
