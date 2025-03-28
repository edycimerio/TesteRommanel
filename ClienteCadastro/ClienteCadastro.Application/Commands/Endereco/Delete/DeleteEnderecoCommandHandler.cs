using System;
using System.Threading;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Commands.Endereco.Delete
{
    public class DeleteEnderecoCommandHandler : IRequestHandler<DeleteEnderecoCommand, bool>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventStore _eventStore;

        public DeleteEnderecoCommandHandler(
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

        public async Task<bool> Handle(DeleteEnderecoCommand request, CancellationToken cancellationToken)
        {
            // Buscar o endereço existente
            var endereco = await _enderecoRepository.GetByIdAsync(request.Id);
            if (endereco == null)
                throw new InvalidOperationException($"Endereço com ID {request.Id} não encontrado");

            // Buscar o cliente associado ao endereço
            var cliente = await _clienteRepository.GetClienteComEnderecosAsync(endereco.ClienteId);
            if (cliente == null)
                throw new InvalidOperationException($"Cliente com ID {endereco.ClienteId} não encontrado");

            // Remover o endereço do cliente
            cliente.RemoverEndereco(endereco);

            // Remover o endereço diretamente do repositório
            await _enderecoRepository.RemoveAsync(endereco);
            
            // Salvar alterações
            var result = await _unitOfWork.CommitAsync();

            // Registrar evento
            var enderecoRemovidoEvent = new EnderecoRemovidoEvent(endereco.Id, endereco.ClienteId);
            await _eventStore.SaveEventAsync(enderecoRemovidoEvent, cliente.Id, "Cliente", 1);

            return result;
        }
    }

    public class EnderecoRemovidoEvent : Event
    {
        public Guid EnderecoId { get; private set; }
        public Guid ClienteId { get; private set; }

        public EnderecoRemovidoEvent(Guid enderecoId, Guid clienteId)
        {
            EnderecoId = enderecoId;
            ClienteId = clienteId;
            AggregateId = clienteId;
        }
    }
}
