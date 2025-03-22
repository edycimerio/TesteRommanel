using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClienteCadastro.Application.Commands.Endereco.Delete;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClienteCadastro.Tests.Commands.Endereco.Delete
{
    public class DeleteEnderecoCommandHandlerTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IEnderecoRepository> _enderecoRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly DeleteEnderecoCommandHandler _handler;

        public DeleteEnderecoCommandHandlerTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _enderecoRepositoryMock = new Mock<IEnderecoRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventStoreMock = new Mock<IEventStore>();
            _handler = new DeleteEnderecoCommandHandler(
                _clienteRepositoryMock.Object,
                _enderecoRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _eventStoreMock.Object);
        }

        [Fact]
        public async Task Handle_EnderecoExistente_DeveRemoverEnderecoERetornarTrue()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var enderecoId = Guid.NewGuid();
            var command = new DeleteEnderecoCommand { Id = enderecoId };
            
            // Criar um cliente usando o construtor adequado
            var cliente = new Domain.Entities.Cliente(
                "Empresa Teste", 
                "12345678901234", 
                "123456789", // IE
                false, // IsIsentoIE
                DateTime.Now.AddYears(-5), // Data de fundação
                "11999999999", // Telefone
                "empresa@teste.com" // Email
            );
            
            // Configurar o ID do cliente
            typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(cliente, clienteId);
            
            // Criar um endereço usando o construtor adequado
            var endereco = new Domain.Entities.Endereco(
                clienteId,
                "12345678",
                "Rua Teste",
                "123",
                "Bairro",
                "Cidade",
                "UF"
            );
            
            // Configurar o ID do endereço
            typeof(Domain.Entities.Endereco).GetProperty("Id").SetValue(endereco, enderecoId);
            
            // Adicionar o endereço à lista de endereços do cliente
            cliente.AdicionarEndereco(endereco);
            
            _enderecoRepositoryMock.Setup(r => r.GetByIdAsync(enderecoId))
                .ReturnsAsync(endereco);

            _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                .ReturnsAsync(cliente);

            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(true);
                
            // Configurar o EventStore para aceitar qualquer evento
            _eventStoreMock.Setup(e => e.SaveEventAsync(It.IsAny<EnderecoRemovidoEvent>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _enderecoRepositoryMock.Verify(r => r.GetByIdAsync(enderecoId), Times.Once);
            _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            _enderecoRepositoryMock.Verify(r => r.RemoveAsync(endereco), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _eventStoreMock.Verify(e => e.SaveEventAsync(It.IsAny<EnderecoRemovidoEvent>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Theory]
        [InlineData("Endereço não encontrado")]
        [InlineData("Cliente não encontrado")]
        public async Task Handle_CenarioInvalido_DeveLancarExcecao(string cenario)
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var enderecoId = Guid.NewGuid();
            var command = new DeleteEnderecoCommand { Id = enderecoId };

            if (cenario == "Endereço não encontrado")
            {
                _enderecoRepositoryMock.Setup(r => r.GetByIdAsync(enderecoId))
                    .ReturnsAsync((Domain.Entities.Endereco)null);
            }
            else if (cenario == "Cliente não encontrado")
            {
                // Criar endereço usando reflection
                var endereco = Activator.CreateInstance(typeof(Domain.Entities.Endereco), true) as Domain.Entities.Endereco;
                
                // Definir propriedades necessárias do endereço
                typeof(Domain.Entities.Endereco).GetProperty("Id").SetValue(endereco, enderecoId);
                typeof(Domain.Entities.Endereco).GetProperty("ClienteId").SetValue(endereco, clienteId);

                _enderecoRepositoryMock.Setup(r => r.GetByIdAsync(enderecoId))
                    .ReturnsAsync(endereco);
                
                _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                    .ReturnsAsync((Domain.Entities.Cliente)null);
            }

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));
                
            exception.Message.Should().Contain(cenario);
            
            if (cenario == "Endereço não encontrado")
            {
                _enderecoRepositoryMock.Verify(r => r.GetByIdAsync(enderecoId), Times.Once);
                _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(It.IsAny<Guid>()), Times.Never);
            }
            else if (cenario == "Cliente não encontrado")
            {
                _enderecoRepositoryMock.Verify(r => r.GetByIdAsync(enderecoId), Times.Once);
                _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            }
            
            _enderecoRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<Domain.Entities.Endereco>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
