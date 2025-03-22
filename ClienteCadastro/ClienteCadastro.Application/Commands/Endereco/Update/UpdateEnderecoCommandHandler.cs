using System;
using System.Linq;
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
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventStore _eventStore;

        public UpdateEnderecoCommandHandler(
            IEnderecoRepository enderecoRepository,
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork,
            IEventStore eventStore)
        {
            _enderecoRepository = enderecoRepository;
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
            _eventStore = eventStore;
        }

        public async Task<bool> Handle(UpdateEnderecoCommand request, CancellationToken cancellationToken)
        {
            // Buscar o cliente com seus endereços
            var cliente = await _clienteRepository.GetClienteComEnderecosAsync(request.ClienteId);
            if (cliente == null)
                throw new InvalidOperationException($"Cliente com ID {request.ClienteId} não encontrado");

            // Buscar o endereço existente
            var endereco = await _enderecoRepository.GetByIdAsync(request.Id);
            if (endereco == null)
                throw new InvalidOperationException($"Endereço com ID {request.Id} não encontrado");

            // Verificar se o endereço pertence ao cliente informado
            if (endereco.ClienteId != request.ClienteId)
                throw new InvalidOperationException("Este endereço não pertence ao cliente informado");

            // Verificar se o endereço existe na lista de endereços do cliente
            var enderecoCliente = cliente.Enderecos.FirstOrDefault(e => e.Id == request.Id);
            if (enderecoCliente == null)
                throw new InvalidOperationException("Este endereço não pertence ao cliente informado");

            // Atualizar o endereço
            endereco.Atualizar(
                request.CEP,
                request.Logradouro,
                request.Numero,
                request.Bairro,
                request.Cidade,
                request.Estado
            );

            try
            {
                // Salvar alterações
                await _enderecoRepository.UpdateAsync(endereco);
                var success = await _unitOfWork.CommitAsync();

                if (success)
                {
                    // Registrar evento
                    var evento = new EnderecoAtualizadoEvent(
                        endereco.Id,
                        endereco.ClienteId,
                        endereco.CEP,
                        endereco.Logradouro,
                        endereco.Numero,
                        endereco.Bairro,
                        endereco.Cidade,
                        endereco.Estado
                    );

                    await _eventStore.SaveEventAsync(evento, endereco.ClienteId, "Endereco", 1);
                }

                return success;
            }
            catch
            {
                return false;
            }
        }
    }

    public class EnderecoAtualizadoEvent : Event
    {
        public Guid EnderecoId { get; private set; }
        public Guid ClienteId { get; private set; }
        public string CEP { get; private set; }
        public string Logradouro { get; private set; }
        public string Numero { get; private set; }
        public string Bairro { get; private set; }
        public string Cidade { get; private set; }
        public string Estado { get; private set; }

        public EnderecoAtualizadoEvent(Guid enderecoId, Guid clienteId, string cep, string logradouro, string numero, string bairro, string cidade, string estado)
        {
            EnderecoId = enderecoId;
            ClienteId = clienteId;
            CEP = cep;
            Logradouro = logradouro;
            Numero = numero;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            AggregateId = clienteId;
        }
    }
}
