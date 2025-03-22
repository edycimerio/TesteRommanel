using System;
using System.Threading;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Commands.Endereco.Insert
{
    public class CreateEnderecoCommandHandler : IRequestHandler<CreateEnderecoCommand, Guid>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventStore _eventStore;

        public CreateEnderecoCommandHandler(
            IClienteRepository clienteRepository,
            IEnderecoRepository enderecoRepository,
            IUnitOfWork unitOfWork,
            IEventStore eventStore)
        {
            _clienteRepository = clienteRepository;
            _enderecoRepository = enderecoRepository;
            _unitOfWork = unitOfWork;
            _eventStore = eventStore;
        }

        public async Task<Guid> Handle(CreateEnderecoCommand request, CancellationToken cancellationToken)
        {
            // Verificar se o cliente existe
            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
            if (cliente == null)
                throw new InvalidOperationException($"Cliente com ID {request.ClienteId} não encontrado");

            // Criar novo endereço
            var endereco = new Domain.Entities.Endereco(
                request.ClienteId,
                request.CEP,
                request.Logradouro,
                request.Numero,
                request.Bairro,
                request.Cidade,
                request.Estado,
                request.Complemento
            );

            // Adicionar endereço ao cliente
            cliente.AdicionarEndereco(endereco);

            // Salvar alterações
            await _clienteRepository.UpdateAsync(cliente);
            await _unitOfWork.CommitAsync();

            // Registrar evento
            await _eventStore.SaveEventAsync(
                new EnderecoAdicionadoEvent(endereco.Id, endereco.ClienteId),
                cliente.Id,
                "Cliente",
                cliente.Version + 1
            );

            return endereco.Id;
        }
    }

    public class EnderecoAdicionadoEvent : Event
    {
        public Guid EnderecoId { get; private set; }
        public Guid ClienteId { get; private set; }

        public EnderecoAdicionadoEvent(Guid enderecoId, Guid clienteId)
        {
            EnderecoId = enderecoId;
            ClienteId = clienteId;
            AggregateId = clienteId;
        }
    }
}
