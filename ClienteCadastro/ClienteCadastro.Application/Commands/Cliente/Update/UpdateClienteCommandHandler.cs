using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace ClienteCadastro.Application.Commands.Cliente.Update
{
    public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, bool>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEventStore _eventStore;
        private readonly IEnderecoRepository _enderecoRepository;

        public UpdateClienteCommandHandler(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEventStore eventStore,
            IEnderecoRepository enderecoRepository)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _eventStore = eventStore;
            _enderecoRepository = enderecoRepository;
        }

        public async Task<bool> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
        {
            // Buscar o cliente
            var cliente = await _clienteRepository.GetClienteComEnderecosAsync(request.Id);
            if (cliente == null)
                throw new InvalidOperationException($"Cliente com ID {request.Id} não encontrado");

            // Verificar se já existe outro cliente com o mesmo documento ou email
            if (await _clienteRepository.DocumentoExisteAsync(request.Documento, request.Id))
                throw new InvalidOperationException($"Já existe outro cliente cadastrado com o documento {request.Documento}");

            if (await _clienteRepository.EmailExisteAsync(request.Email, request.Id))
                throw new InvalidOperationException($"Já existe outro cliente cadastrado com o email {request.Email}");

            // Atualizar dados do cliente
            cliente.AtualizarDados(
                request.TipoPessoa,
                request.Nome,
                request.Documento,
                request.IE ?? string.Empty,
                request.IsIsentoIE,
                request.DataNascimento,
                request.Telefone,
                request.Email
            );

            // Atualizar status de ativo/inativo
            if (request.Ativo)
                cliente.Ativar();
            else
                cliente.Desativar();

            // Atualizar endereços
            // Primeiro, vamos identificar os IDs dos endereços que estão vindo na requisição
            var idsEnderecosRequest = request.Enderecos
                .Where(e => e.Id != Guid.Empty)
                .Select(e => e.Id)
                .ToList();
            
            // Endereços atuais do cliente
            var enderecosAtuais = cliente.Enderecos.ToList();
            
            // Remover endereços que não estão na requisição
            var enderecosParaRemover = enderecosAtuais
                .Where(e => !idsEnderecosRequest.Contains(e.Id))
                .ToList();
                
            foreach (var endereco in enderecosParaRemover)
            {
                cliente.RemoverEndereco(endereco);
            }
            
            // Atualizar endereços existentes e adicionar novos
            foreach (var enderecoDto in request.Enderecos)
            {
                if (enderecoDto.Id != Guid.Empty)
                {
                    // Buscar o endereço existente
                    var enderecoExistente = await _enderecoRepository.GetByIdAsync(enderecoDto.Id);
                    if (enderecoExistente != null)
                    {
                        // Atualizar o endereço existente usando o método Atualizar
                        enderecoExistente.Atualizar(
                            enderecoDto.CEP,
                            enderecoDto.Logradouro,
                            enderecoDto.Numero,
                            enderecoDto.Bairro,
                            enderecoDto.Cidade,
                            enderecoDto.Estado,
                            enderecoDto.Complemento
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
                        enderecoDto.Estado,
                        enderecoDto.Complemento
                    );
                    
                    cliente.AdicionarEndereco(novoEndereco);
                }
            }

            // Salvar as alterações
            await _clienteRepository.UpdateAsync(cliente);
            await _unitOfWork.CommitAsync();

            // Registrar evento
            await _eventStore.SaveEventAsync(
                new ClienteAtualizadoEvent(cliente.Id, cliente.Nome, cliente.Documento, cliente.Email),
                cliente.Id,
                "Cliente",
                cliente.Version + 1
            );

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
            AggregateId = clienteId;
            ClienteId = clienteId;
            Nome = nome;
            Documento = documento;
            Email = email;
        }
    }
}
