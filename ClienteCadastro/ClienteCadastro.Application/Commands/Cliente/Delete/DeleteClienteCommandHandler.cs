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
            // Buscar o cliente
            var cliente = await _clienteRepository.GetByIdAsync(request.Id);
            if (cliente == null)
                throw new InvalidOperationException($"Cliente com ID {request.Id} não encontrado");

            // Remover o cliente
            await _clienteRepository.RemoveAsync(cliente);
            await _unitOfWork.CommitAsync();

            // Registrar evento
            await _eventStore.SaveEventAsync(
                new ClienteRemovidoEvent(cliente.Id, cliente.Nome),
                cliente.Id,
                "Cliente",
                cliente.Version + 1
            );

            return true;
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
