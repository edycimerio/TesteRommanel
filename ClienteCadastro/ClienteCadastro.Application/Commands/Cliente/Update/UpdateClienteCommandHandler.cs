using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using MediatR;
using System.Linq;
using ClienteCadastro.Application.DTOs;

namespace ClienteCadastro.Application.Commands.Cliente.Update
{
    public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, bool>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventStore _eventStore;
        private readonly IMapper _mapper;

        public UpdateClienteCommandHandler(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork,
            IEventStore eventStore,
            IMapper mapper)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
            _eventStore = eventStore;
            _mapper = mapper;
        }

        public async Task<bool> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
        {
            // Buscar o cliente existente
            var cliente = await _clienteRepository.GetClienteComEnderecosAsync(request.Id);
            if (cliente == null)
                throw new InvalidOperationException($"Cliente com ID {request.Id} não encontrado");

            // Verificar se o documento já existe para outro cliente
            if (!string.Equals(cliente.Documento, request.Documento, StringComparison.OrdinalIgnoreCase))
            {
                var clienteExistente = await _clienteRepository.GetByDocumentoAsync(request.Documento);
                if (clienteExistente != null && clienteExistente.Id != cliente.Id)
                    throw new InvalidOperationException($"Já existe um cliente cadastrado com o documento {request.Documento}");
            }

            // Verificar se o email já existe para outro cliente
            if (!string.Equals(cliente.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var clienteExistente = await _clienteRepository.GetByEmailAsync(request.Email);
                if (clienteExistente != null && clienteExistente.Id != cliente.Id)
                    throw new InvalidOperationException($"Já existe um cliente cadastrado com o email {request.Email}");
            }

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

            // Atualizar dados do cliente
            cliente.Atualizar(
                request.TipoPessoa,
                request.Nome,
                request.Documento,
                request.IE,
                request.IsIsentoIE,
                request.DataNascimento,
                request.Telefone,
                request.Email
            );

            // Atualizar endereços
            if (request.Enderecos != null && request.Enderecos.Any())
            {
                // Obter IDs dos endereços existentes
                var enderecosExistentesIds = cliente.Enderecos.Select(e => e.Id).ToList();
                
                // Obter IDs dos endereços da requisição
                var enderecosRequisicaoIds = request.Enderecos
                    .Where(e => e.Id != Guid.Empty)
                    .Select(e => e.Id)
                    .ToList();
                
                // Identificar endereços a serem removidos (existem no cliente mas não na requisição)
                var enderecosParaRemover = cliente.Enderecos
                    .Where(e => !enderecosRequisicaoIds.Contains(e.Id))
                    .ToList();
                
                // Remover endereços
                foreach (var endereco in enderecosParaRemover)
                {
                    cliente.RemoverEndereco(endereco.Id);
                }

                // Atualizar ou adicionar endereços
                foreach (var enderecoDto in request.Enderecos)
                {
                    if (enderecoDto.Id != Guid.Empty && enderecosExistentesIds.Contains(enderecoDto.Id))
                    {
                        // Atualizar endereço existente
                        var endereco = cliente.Enderecos.FirstOrDefault(e => e.Id == enderecoDto.Id);
                        if (endereco != null)
                        {
                            endereco.Atualizar(
                                enderecoDto.CEP,
                                enderecoDto.Logradouro,
                                enderecoDto.Numero,
                                enderecoDto.Bairro,
                                enderecoDto.Cidade,
                                enderecoDto.Estado
                            );
                        }
                    }
                    else
                    {
                        // Adicionar novo endereço
                        var novoEndereco = new Domain.Entities.Endereco(
                            cliente.Id,
                            enderecoDto.CEP,
                            enderecoDto.Logradouro,
                            enderecoDto.Numero,
                            enderecoDto.Bairro,
                            enderecoDto.Cidade,
                            enderecoDto.Estado
                        );
                        
                        cliente.AdicionarEndereco(novoEndereco);
                    }
                }
            }

            // Salvar as alterações
            await _clienteRepository.UpdateAsync(cliente);
            await _unitOfWork.CommitAsync();

            // Registrar evento
            var clienteAtualizadoEvent = new ClienteAtualizadoEvent(
                cliente.Id, 
                cliente.Nome, 
                cliente.Documento, 
                cliente.Email
            );
            
            await _eventStore.SaveEventAsync(clienteAtualizadoEvent, cliente.Id, "Cliente", 1);

            return true;
        }
    }

    public class ClienteAtualizadoEvent : Event
    {
        public Guid ClienteId { get; private set; }
        public string Nome { get; private set; }
        public string Documento { get; private set; }
        public string Email { get; private set; }

        public ClienteAtualizadoEvent(Guid clienteId, string nome, string documento, string email)
        {
            ClienteId = clienteId;
            Nome = nome;
            Documento = documento;
            Email = email;
            AggregateId = clienteId;
        }
    }
}
