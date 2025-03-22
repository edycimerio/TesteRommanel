using System;
using System.Threading;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Commands.Cliente.Delete
{
    public class DeleteClienteCommandHandler : IRequestHandler<DeleteClienteCommand, bool>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventStore _eventStore;

        public DeleteClienteCommandHandler(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork,
            IEventStore eventStore)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
            _eventStore = eventStore;
        }

        public async Task<bool> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
        {
            // Buscar o cliente com seus endereços
            var cliente = await _clienteRepository.GetClienteComEnderecosAsync(request.Id);
            if (cliente == null)
                throw new InvalidOperationException($"Cliente com ID {request.Id} não encontrado");

            // Remover o cliente (isso também removerá os endereços devido ao Cascade Delete)
            await _clienteRepository.RemoveAsync(cliente);
            
            // Garantir que as alterações sejam salvas no banco de dados
            var result = await _unitOfWork.CommitAsync();

            // Registrar evento
            var clienteRemovidoEvent = new ClienteRemovidoEvent(cliente.Id, cliente.Nome);
            await _eventStore.SaveEventAsync(clienteRemovidoEvent, cliente.Id, "Cliente", 1);

            return result;
        }
    }

    public class ClienteRemovidoEvent : Event
    {
        public Guid ClienteId { get; private set; }
        public string Nome { get; private set; }

        public ClienteRemovidoEvent(Guid clienteId, string nome)
        {
            AggregateId = clienteId;
            ClienteId = clienteId;
            Nome = nome;
        }
    }
}
