using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClienteCadastro.Application.Commands.Endereco.Insert;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClienteCadastro.Tests.Commands.Endereco.Insert
{
    public class CreateEnderecoCommandHandlerTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IEnderecoRepository> _enderecoRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly CreateEnderecoCommandHandler _handler;

        public CreateEnderecoCommandHandlerTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _enderecoRepositoryMock = new Mock<IEnderecoRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventStoreMock = new Mock<IEventStore>();
            _handler = new CreateEnderecoCommandHandler(
                _clienteRepositoryMock.Object,
                _enderecoRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _eventStoreMock.Object);
        }

        [Fact]
        public async Task Handle_EnderecoValido_DeveCriarEnderecoERetornarId()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var novoEnderecoId = Guid.NewGuid();
            var command = new CreateEnderecoCommand
            {
                ClienteId = clienteId,
                CEP = "12345678",
                Logradouro = "Rua Teste",
                Numero = "123",
                Bairro = "Bairro Teste",
                Cidade = "Cidade Teste",
                Estado = "SP"
            };

            // Criar cliente usando o construtor adequado
            var cliente = new Domain.Entities.Cliente(
                "Cliente Teste", 
                "12345678901", 
                DateTime.Now.AddYears(-20), // Data de nascimento (pessoa física)
                "11999999999", // Telefone
                "cliente@teste.com" // Email
            );
            
            // Configurar o ID do cliente usando reflection (isso ainda é necessário)
            typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(cliente, clienteId);

            _clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId))
                .ReturnsAsync(cliente);

            _enderecoRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Endereco>()))
                .Callback<Domain.Entities.Endereco>(endereco => 
                {
                    // Definir o ID do endereço usando reflection
                    typeof(Domain.Entities.Endereco).GetProperty("Id").SetValue(endereco, novoEnderecoId);
                    
                    // Não adicionar o endereço à lista de endereços do cliente aqui
                    // O método AdicionarEndereco será chamado pelo handler
                })
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(true);

            // Configurar o EventStore para aceitar qualquer evento
            _eventStoreMock.Setup(e => e.SaveEventAsync(It.IsAny<EnderecoAdicionadoEvent>(), clienteId, "Endereco", 1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty(); // Verificamos apenas que o ID não é vazio
            _clienteRepositoryMock.Verify(r => r.GetByIdAsync(clienteId), Times.Once);
            _enderecoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Endereco>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            // Verificar se o evento foi salvo
            _eventStoreMock.Verify(e => e.SaveEventAsync(
                It.IsAny<EnderecoAdicionadoEvent>(),
                clienteId,
                "Endereco",
                1
            ), Times.Once);
            
            // Verificar se o endereço foi adicionado à lista de endereços do cliente
            cliente.Enderecos.Should().HaveCount(1);
            cliente.Enderecos.First().ClienteId.Should().Be(clienteId);
        }

        [Fact]
        public async Task Handle_ClienteNaoExistente_DeveLancarExcecao()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var command = new CreateEnderecoCommand
            {
                ClienteId = clienteId,
                CEP = "12345678",
                Logradouro = "Rua Teste",
                Numero = "123",
                Bairro = "Bairro Teste",
                Cidade = "Cidade Teste",
                Estado = "SP"
            };

            _clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId))
                .ReturnsAsync((Domain.Entities.Cliente)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _clienteRepositoryMock.Verify(r => r.GetByIdAsync(clienteId), Times.Once);
            _enderecoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Endereco>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Theory]
        [InlineData("", "Rua Teste", "123", "Bairro Teste", "Cidade Teste", "SP", "CEP não pode ser vazio")]
        [InlineData("12345678", "", "123", "Bairro Teste", "Cidade Teste", "SP", "Logradouro não pode ser vazio")]
        [InlineData("12345678", "Rua Teste", "", "Bairro Teste", "Cidade Teste", "SP", "Número não pode ser vazio")]
        [InlineData("12345678", "Rua Teste", "123", "", "Cidade Teste", "SP", "Bairro não pode ser vazio")]
        [InlineData("12345678", "Rua Teste", "123", "Bairro Teste", "", "SP", "Cidade não pode ser vazia")]
        [InlineData("12345678", "Rua Teste", "123", "Bairro Teste", "Cidade Teste", "", "Estado não pode ser vazio")]
        public async Task Handle_DadosInvalidos_DeveLancarExcecao(
            string cep, string logradouro, string numero, string bairro, 
            string cidade, string estado, string mensagemEsperada)
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var command = new CreateEnderecoCommand
            {
                ClienteId = clienteId,
                CEP = cep,
                Logradouro = logradouro,
                Numero = numero,
                Bairro = bairro,
                Cidade = cidade,
                Estado = estado
            };

            // Criar cliente usando o construtor adequado
            var cliente = new Domain.Entities.Cliente(
                "Cliente Teste", 
                "12345678901", 
                DateTime.Now.AddYears(-20), // Data de nascimento (pessoa física)
                "11999999999", // Telefone
                "cliente@teste.com" // Email
            );
            
            // Configurar o ID do cliente usando reflection
            typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(cliente, clienteId);

            _clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId))
                .ReturnsAsync(cliente);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));
                
            exception.Message.Should().Contain(mensagemEsperada);
            _enderecoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Endereco>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
