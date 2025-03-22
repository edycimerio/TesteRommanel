using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Application.Commands.Cliente.Update;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClienteCadastro.Tests.Commands.Cliente.Update
{
    public class UpdateClienteCommandHandlerTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateClienteCommandHandler _handler;

        public UpdateClienteCommandHandlerTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventStoreMock = new Mock<IEventStore>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateClienteCommandHandler(
                _clienteRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _eventStoreMock.Object,
                _mapperMock.Object);
        }

        [Theory]
        [InlineData('F', "Pessoa Física")]
        [InlineData('J', "Pessoa Jurídica")]
        public async Task Handle_ClienteValido_DeveAtualizarClienteERetornarTrue(char tipoPessoa, string descricao)
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var command = new UpdateClienteCommand
            {
                Id = clienteId,
                TipoPessoa = tipoPessoa,
                Nome = tipoPessoa == 'F' ? "João Silva Atualizado" : "Empresa Teste Atualizada",
                Documento = tipoPessoa == 'F' ? "12345678900" : "12345678901234",
                IE = tipoPessoa == 'F' ? null : "987654321",
                IsIsentoIE = tipoPessoa == 'F' ? false : false,
                DataNascimento = tipoPessoa == 'F' ? DateTime.Now.AddYears(-30) : DateTime.Now.AddYears(-5),
                Telefone = "11999999999",
                Email = tipoPessoa == 'F' ? "joao.atualizado@teste.com" : "empresa.atualizada@teste.com"
            };

            // Criar cliente existente usando reflection
            var clienteExistente = Activator.CreateInstance(typeof(Domain.Entities.Cliente), true) as Domain.Entities.Cliente;
            
            // Definir propriedades necessárias
            typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(clienteExistente, clienteId);
            typeof(Domain.Entities.Cliente).GetProperty("TipoPessoa").SetValue(clienteExistente, tipoPessoa);
            typeof(Domain.Entities.Cliente).GetProperty("Nome").SetValue(clienteExistente, tipoPessoa == 'F' ? "João Silva" : "Empresa Teste");
            typeof(Domain.Entities.Cliente).GetProperty("Documento").SetValue(clienteExistente, tipoPessoa == 'F' ? "12345678900" : "12345678901234");
            typeof(Domain.Entities.Cliente).GetProperty("Email").SetValue(clienteExistente, tipoPessoa == 'F' ? "joao@teste.com" : "empresa@teste.com");
            typeof(Domain.Entities.Cliente).GetProperty("DataNascimento").SetValue(clienteExistente, tipoPessoa == 'F' ? DateTime.Now.AddYears(-30) : DateTime.Now.AddYears(-5));
            
            if (tipoPessoa == 'J')
            {
                typeof(Domain.Entities.Cliente).GetProperty("IE").SetValue(clienteExistente, "123456789");
                typeof(Domain.Entities.Cliente).GetProperty("IsIsentoIE").SetValue(clienteExistente, false);
            }

            _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                .ReturnsAsync(clienteExistente);

            _clienteRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email))
                .ReturnsAsync((Domain.Entities.Cliente)null);

            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(true);

            _eventStoreMock.Setup(e => e.SaveEventAsync(It.IsAny<ClienteAtualizadoEvent>(), clienteId, "Cliente", 1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            _clienteRepositoryMock.Verify(r => r.GetByEmailAsync(command.Email), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _eventStoreMock.Verify(e => e.SaveEventAsync(It.IsAny<ClienteAtualizadoEvent>(), clienteId, "Cliente", 1), Times.Once);
        }

        [Theory]
        [InlineData("ClienteNaoEncontrado", "Cliente não encontrado")]
        [InlineData("EmailJaExistente", "E-mail já cadastrado")]
        [InlineData("PessoaFisicaMenorDeIdade", "Cliente deve ter pelo menos 18 anos")]
        [InlineData("PessoaJuridicaSemIE", "Inscrição Estadual é obrigatória")]
        public async Task Handle_ClienteInvalido_DeveLancarExcecao(string cenarioInvalido, string mensagemEsperada)
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var outroClienteId = Guid.NewGuid();
            var command = new UpdateClienteCommand
            {
                Id = clienteId,
                TipoPessoa = cenarioInvalido == "PessoaJuridicaSemIE" ? 'J' : 'F',
                Nome = "Teste Atualizado",
                Documento = "12345678900",
                IE = cenarioInvalido == "PessoaJuridicaSemIE" ? null : null,
                IsIsentoIE = cenarioInvalido == "PessoaJuridicaSemIE" ? false : false,
                DataNascimento = cenarioInvalido == "PessoaFisicaMenorDeIdade" ? DateTime.Now.AddYears(-16) : DateTime.Now.AddYears(-30),
                Telefone = "11999999999",
                Email = "teste.atualizado@teste.com"
            };

            if (cenarioInvalido == "ClienteNaoEncontrado")
            {
                _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                    .ReturnsAsync((Domain.Entities.Cliente)null);
            }
            else if (cenarioInvalido == "EmailJaExistente")
            {
                // Criar cliente existente
                var clienteExistente = Activator.CreateInstance(typeof(Domain.Entities.Cliente), true) as Domain.Entities.Cliente;
                typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(clienteExistente, clienteId);
                typeof(Domain.Entities.Cliente).GetProperty("TipoPessoa").SetValue(clienteExistente, 'F');
                typeof(Domain.Entities.Cliente).GetProperty("Nome").SetValue(clienteExistente, "Cliente Existente");
                typeof(Domain.Entities.Cliente).GetProperty("Documento").SetValue(clienteExistente, "12345678900");
                typeof(Domain.Entities.Cliente).GetProperty("Email").SetValue(clienteExistente, "cliente@teste.com");
                typeof(Domain.Entities.Cliente).GetProperty("DataNascimento").SetValue(clienteExistente, DateTime.Now.AddYears(-30));
                
                _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                    .ReturnsAsync(clienteExistente);
                
                // Criar outro cliente com o mesmo email
                var outroCliente = Activator.CreateInstance(typeof(Domain.Entities.Cliente), true) as Domain.Entities.Cliente;
                typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(outroCliente, outroClienteId);
                typeof(Domain.Entities.Cliente).GetProperty("Email").SetValue(outroCliente, command.Email);
                
                _clienteRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email))
                    .ReturnsAsync(outroCliente);
            }
            else
            {
                // Criar cliente existente para os outros cenários
                var clienteExistente = Activator.CreateInstance(typeof(Domain.Entities.Cliente), true) as Domain.Entities.Cliente;
                typeof(Domain.Entities.Cliente).GetProperty("Id").SetValue(clienteExistente, clienteId);
                typeof(Domain.Entities.Cliente).GetProperty("TipoPessoa").SetValue(clienteExistente, command.TipoPessoa);
                typeof(Domain.Entities.Cliente).GetProperty("Nome").SetValue(clienteExistente, "Cliente Existente");
                typeof(Domain.Entities.Cliente).GetProperty("Documento").SetValue(clienteExistente, "12345678900");
                typeof(Domain.Entities.Cliente).GetProperty("Email").SetValue(clienteExistente, "cliente@teste.com");
                typeof(Domain.Entities.Cliente).GetProperty("DataNascimento").SetValue(clienteExistente, DateTime.Now.AddYears(-30));
                
                _clienteRepositoryMock.Setup(r => r.GetClienteComEnderecosAsync(clienteId))
                    .ReturnsAsync(clienteExistente);
                
                _clienteRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email))
                    .ReturnsAsync((Domain.Entities.Cliente)null);
            }

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));
                
            exception.Message.Should().Contain(mensagemEsperada);
            
            if (cenarioInvalido != "ClienteNaoEncontrado")
            {
                _clienteRepositoryMock.Verify(r => r.GetClienteComEnderecosAsync(clienteId), Times.Once);
            }
            
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
