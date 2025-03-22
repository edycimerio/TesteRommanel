using System;
using System.Threading;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using MediatR;

namespace ClienteCadastro.Application.Commands.Endereco.Update
{
    public class UpdateEnderecoCommandHandler : IRequestHandler<UpdateEnderecoCommand, bool>
    {
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventStore _eventStore;

        public UpdateEnderecoCommandHandler(
            IEnderecoRepository enderecoRepository,
            IUnitOfWork unitOfWork,
            IEventStore eventStore)
        {
            _enderecoRepository = enderecoRepository;
            _unitOfWork = unitOfWork;
            _eventStore = eventStore;
        }

        public async Task<bool> Handle(UpdateEnderecoCommand request, CancellationToken cancellationToken)
        {
            // Buscar o endereço existente
            var endereco = await _enderecoRepository.GetByIdAsync(request.Id);
            if (endereco == null)
                throw new InvalidOperationException($"Endereço com ID {request.Id} não encontrado");

            // Verificar se o endereço pertence ao cliente informado
            if (endereco.ClienteId != request.ClienteId)
                throw new InvalidOperationException("Este endereço não pertence ao cliente informado");

            // Atualizar o endereço
            endereco.Atualizar(
                request.CEP,
                request.Logradouro,
                request.Numero,
                request.Bairro,
                request.Cidade,
                request.Estado,
                request.Complemento
            );

            // Salvar alterações
            await _enderecoRepository.UpdateAsync(endereco);
            await _unitOfWork.CommitAsync();

            // Registrar evento
            await _eventStore.SaveEventAsync(
                new EnderecoAtualizadoEvent(endereco.Id, endereco.ClienteId),
                endereco.ClienteId,
                "Cliente",
                0 // A versão será incrementada pelo EventStore
            );

            return true;
        }
    }

    public class EnderecoAtualizadoEvent : Event
    {
        public Guid EnderecoId { get; private set; }
        public Guid ClienteId { get; private set; }

        public EnderecoAtualizadoEvent(Guid enderecoId, Guid clienteId)
        {
            EnderecoId = enderecoId;
            ClienteId = clienteId;
            AggregateId = clienteId;
        }
    }
}
