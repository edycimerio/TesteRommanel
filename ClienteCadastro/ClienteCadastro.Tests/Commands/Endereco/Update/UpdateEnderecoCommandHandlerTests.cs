using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClienteCadastro.Application.Commands.Endereco.Update;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClienteCadastro.Tests.Commands.Endereco.Update
{
    public class UpdateEnderecoCommandHandlerTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IEnderecoRepository> _enderecoRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly UpdateEnderecoCommandHandler _handler;

        public UpdateEnderecoCommandHandlerTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _enderecoRepositoryMock = new Mock<IEnderecoRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventStoreMock = new Mock<IEventStore>();
            _handler = new UpdateEnderecoCommandHandler(
                _enderecoRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _eventStoreMock.Object);
        }

        [Fact]
        public async Task Handle_EnderecoValido_DeveAtualizarEnderecoERetornarTrue()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var enderecoId = Guid.NewGuid();
            var command = new UpdateEnderecoCommand
            {
                Id = enderecoId,
                ClienteId = clienteId,
                CEP = "87654321", // CEP atualizado
                Logradouro = "Rua Atualizada",
                Numero = "456",
                Bairro = "Bairro Atualizado",
                Cidade = "Cidade Atualizada",
                Estado = "RJ"
            };

            // Criar um cliente usando o construtor adequado
            var cliente = new Domain.Entities.Cliente(
                "Cliente Teste", 
                "12345678901", 
                DateTime.Now.AddYears(-20), // Data de nascimento (pessoa física)
                "11999999999", // Telefone
                "cliente@teste.com" // Email
            );
            
            // Configurar o ID do cliente
            typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(cliente, clienteId);
            
            // Criar um endereço usando o construtor adequado
            var endereco = new Domain.Entities.Endereco(
                clienteId,
                "12345678",
                "Rua Teste",
                "123",
                "Bairro Teste",
                "Cidade Teste",
                "SP"
            );
            
            // Configurar o ID do endereço
            typeof(Domain.Entities.Endereco).GetProperty("Id").SetValue(endereco, enderecoId);
            
            // Adicionar o endereço à lista de endereços do cliente
            cliente.AdicionarEndereco(endereco);

            _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                .ReturnsAsync(cliente);

            _enderecoRepositoryMock.Setup(r => r.GetByIdAsync(enderecoId))
                .ReturnsAsync(endereco);

            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(true);
                
            // Configurar o EventStore para aceitar qualquer evento
            _eventStoreMock.Setup(e => e.SaveEventAsync(It.IsAny<EnderecoAtualizadoEvent>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            
            _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            _enderecoRepositoryMock.Verify(r => r.GetByIdAsync(enderecoId), Times.Once);
            _enderecoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Endereco>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _eventStoreMock.Verify(e => e.SaveEventAsync(It.IsAny<EnderecoAtualizadoEvent>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EnderecoNaoExistente_DeveLancarExcecao()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var enderecoId = Guid.NewGuid();
            var command = new UpdateEnderecoCommand
            {
                Id = enderecoId,
                ClienteId = clienteId,
                CEP = "12345678",
                Logradouro = "Rua Teste",
                Numero = "123",
                Bairro = "Bairro Teste",
                Cidade = "Cidade Teste",
                Estado = "SP"
            };

            // Configurar o mock do cliente para retornar um cliente válido
            var cliente = new Domain.Entities.Cliente(
                "Cliente Teste", 
                "12345678901", 
                DateTime.Now.AddYears(-20),
                "11999999999",
                "cliente@teste.com"
            );
            typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(cliente, clienteId);
            
            _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                .ReturnsAsync(cliente);

            // Configurar o mock do endereço para retornar null (endereço não existe)
            _enderecoRepositoryMock.Setup(r => r.GetByIdAsync(enderecoId))
                .ReturnsAsync((Domain.Entities.Endereco)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _enderecoRepositoryMock.Verify(r => r.GetByIdAsync(enderecoId), Times.Once);
            _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            _enderecoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Endereco>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ClienteNaoExistente_DeveLancarExcecao()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var enderecoId = Guid.NewGuid();
            var command = new UpdateEnderecoCommand
            {
                Id = enderecoId,
                ClienteId = clienteId,
                CEP = "87654321",
                Logradouro = "Rua Atualizada",
                Numero = "456",
                Bairro = "Bairro Atualizado",
                Cidade = "Cidade Atualizada",
                Estado = "RJ"
            };

            // Criar um endereço usando o construtor adequado
            var endereco = new Domain.Entities.Endereco(
                clienteId,
                "12345678",
                "Rua Teste",
                "123",
                "Bairro Teste",
                "Cidade Teste",
                "SP"
            );
            
            _enderecoRepositoryMock.Setup(r => r.GetByIdAsync(enderecoId))
                .ReturnsAsync(endereco);

            // Configurar o cliente como não existente
            _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                .ReturnsAsync((Domain.Entities.Cliente)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            _enderecoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Endereco>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Theory]
        [InlineData("", "Rua Teste", "123", "Bairro Teste", "Cidade Teste", "SP", "CEP não pode ser vazio")]
        [InlineData("12345678", "", "123", "Bairro Teste", "Cidade Teste", "SP", "Logradouro não pode ser vazio")]
        [InlineData("12345678", "Rua Teste", "", "Bairro Teste", "Cidade Teste", "SP", "Número não pode ser vazio")]
        [InlineData("12345678", "Rua Teste", "123", "", "Cidade Teste", "SP", "Bairro não pode ser vazio")]
        [InlineData("12345678", "Rua Teste", "123", "Bairro Teste", "", "SP", "Cidade não pode ser vazia")]
        [InlineData("12345678", "Rua Teste", "123", "Bairro Teste", "Cidade Teste", "", "Estado não pode ser vazio")]
        [InlineData("12345678", "Rua Teste", "123", "Bairro Teste", "Cidade Teste", "SP", "Endereço não encontrado")]
        [InlineData("12345678", "Rua Teste", "123", "Bairro Teste", "Cidade Teste", "SP", "Cliente não encontrado")]
        public async Task Handle_DadosInvalidos_DeveLancarExcecao(
            string cep, string logradouro, string numero, string bairro, 
            string cidade, string estado, string mensagemEsperada)
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var enderecoId = Guid.NewGuid();
            var command = new UpdateEnderecoCommand
            {
                Id = enderecoId,
                ClienteId = clienteId,
                CEP = cep,
                Logradouro = logradouro,
                Numero = numero,
                Bairro = bairro,
                Cidade = cidade,
                Estado = estado
            };

            if (mensagemEsperada == "Endereço não encontrado")
            {
                _enderecoRepositoryMock.Setup(r => r.GetByIdAsync(enderecoId))
                    .ReturnsAsync((Domain.Entities.Endereco)null);
            }
            else if (mensagemEsperada == "Cliente não encontrado")
            {
                _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                    .ReturnsAsync((Domain.Entities.Cliente)null);
            }
            else
            {
                // Criar um cliente usando o construtor adequado
                var cliente = new Domain.Entities.Cliente(
                    "Cliente Teste", 
                    "12345678901", 
                    DateTime.Now.AddYears(-20), // Data de nascimento (pessoa física)
                    "11999999999", // Telefone
                    "cliente@teste.com" // Email
                );
                
                // Configurar o ID do cliente
                typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(cliente, clienteId);
                
                // Criar um endereço usando o construtor adequado
                var endereco = new Domain.Entities.Endereco(
                    clienteId,
                    "12345678",
                    "Rua Teste",
                    "123",
                    "Bairro Teste",
                    "Cidade Teste",
                    "SP"
                );
                
                // Configurar o ID do endereço
                typeof(Domain.Entities.Endereco).GetProperty("Id").SetValue(endereco, enderecoId);
                
                // Adicionar o endereço à lista de endereços do cliente
                cliente.AdicionarEndereco(endereco);

                _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                    .ReturnsAsync(cliente);

                _enderecoRepositoryMock.Setup(r => r.GetByIdAsync(enderecoId))
                    .ReturnsAsync(endereco);
            }

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));
                
            exception.Message.Should().Contain(mensagemEsperada);
            _enderecoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Endereco>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ErroAoSalvar_DeveRetornarFalse()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var enderecoId = Guid.NewGuid();
            var command = new UpdateEnderecoCommand
            {
                Id = enderecoId,
                ClienteId = clienteId,
                CEP = "87654321",
                Logradouro = "Rua Atualizada",
                Numero = "456",
                Bairro = "Bairro Atualizado",
                Cidade = "Cidade Atualizada",
                Estado = "RJ"
            };

            // Criar um cliente usando o construtor adequado
            var cliente = new Domain.Entities.Cliente(
                "Cliente Teste", 
                "12345678901", 
                DateTime.Now.AddYears(-20), 
                "11999999999", 
                "cliente@teste.com"
            );
            
            // Configurar o ID do cliente
            typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(cliente, clienteId);

            // Criar um endereço usando o construtor adequado
            var endereco = new Domain.Entities.Endereco(
                clienteId,
                "12345678",
                "Rua Teste",
                "123",
                "Bairro Teste",
                "Cidade Teste",
                "SP"
            );
            
            // Configurar o ID do endereço
            typeof(Domain.Entities.Endereco).GetProperty("Id").SetValue(endereco, enderecoId);
            
            // Adicionar o endereço à lista de endereços do cliente
            cliente.AdicionarEndereco(endereco);

            _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                .ReturnsAsync(cliente);

            _enderecoRepositoryMock.Setup(r => r.GetByIdAsync(enderecoId))
                .ReturnsAsync(endereco);

            // Configurar o UnitOfWork para falhar ao salvar
            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            
            _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            _enderecoRepositoryMock.Verify(r => r.GetByIdAsync(enderecoId), Times.Once);
            _enderecoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Endereco>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _eventStoreMock.Verify(e => e.SaveEventAsync(It.IsAny<EnderecoAtualizadoEvent>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }
    }
}
