using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClienteCadastro.Application.Commands.Cliente.Delete;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClienteCadastro.Tests.Commands.Cliente.Delete
{
    public class DeleteClienteCommandHandlerTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly DeleteClienteCommandHandler _handler;

        public DeleteClienteCommandHandlerTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventStoreMock = new Mock<IEventStore>();
            _handler = new DeleteClienteCommandHandler(
                _clienteRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _eventStoreMock.Object);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task Handle_ClienteExistente_DeveRemoverClienteERetornarResultadoCorreto(bool commitSuccess, bool expectedResult)
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var command = new DeleteClienteCommand { Id = clienteId };
            
            // Criar cliente usando reflection para evitar problemas com construtor
            var cliente = Activator.CreateInstance(typeof(Domain.Entities.Cliente), true) as Domain.Entities.Cliente;
            
            // Definir propriedades necessárias
            typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(cliente, clienteId);
            typeof(Domain.Entities.Cliente).GetProperty("Nome").SetValue(cliente, "Empresa Teste");
            typeof(Domain.Entities.Cliente).GetProperty("Documento").SetValue(cliente, "12345678901234");
            typeof(Domain.Entities.Cliente).GetProperty("Email").SetValue(cliente, "empresa@teste.com");
            
            // Configurar mocks
            _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                .ReturnsAsync(cliente);

            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(commitSuccess);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(expectedResult);
            _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            _clienteRepositoryMock.Verify(r => r.RemoveAsync(cliente), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            
            if (commitSuccess)
            {
                _eventStoreMock.Verify(e => e.SaveEventAsync(It.IsAny<ClienteRemovidoEvent>(), clienteId, "Cliente", 1), Times.Once);
            }
            else
            {
                _eventStoreMock.Verify(e => e.SaveEventAsync(It.IsAny<ClienteRemovidoEvent>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            }
        }

        [Fact]
        public async Task Handle_ClienteNaoExistente_DeveLancarExcecao()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var command = new DeleteClienteCommand { Id = clienteId };

            _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                .ReturnsAsync((Domain.Entities.Cliente)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));
                
            exception.Message.Should().Contain("não encontrado");
            
            _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            _clienteRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<Domain.Entities.Cliente>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
