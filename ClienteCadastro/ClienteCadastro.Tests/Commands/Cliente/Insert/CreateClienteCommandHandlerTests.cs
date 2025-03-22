using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClienteCadastro.Application.Commands.Cliente.Insert;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClienteCadastro.Tests.Commands.Cliente.Insert
{
    public class CreateClienteCommandHandlerTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateClienteCommandHandler _handler;

        public CreateClienteCommandHandlerTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventStoreMock = new Mock<IEventStore>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateClienteCommandHandler(
                _clienteRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _eventStoreMock.Object);
        }

        [Theory]
        [InlineData('F', "Pessoa Física")]
        [InlineData('J', "Pessoa Jurídica")]
        public async Task Handle_ClienteValido_DeveCriarClienteERetornarId(char tipoPessoa, string descricao)
        {
            // Arrange
            var command = new CreateClienteCommand
            {
                TipoPessoa = tipoPessoa,
                Nome = tipoPessoa == 'F' ? "João Silva" : "Empresa Teste",
                Documento = tipoPessoa == 'F' ? "12345678900" : "12345678000190",
                IE = tipoPessoa == 'F' ? null : "123456789",
                IsIsentoIE = tipoPessoa == 'F' ? false : false,
                DataNascimento = tipoPessoa == 'F' ? DateTime.Now.AddYears(-30) : DateTime.MinValue,
                Telefone = "11999999999",
                Email = tipoPessoa == 'F' ? "joao@teste.com" : "empresa@teste.com",
                Enderecos = new List<EnderecoDTO>
                {
                    new EnderecoDTO
                    {
                        CEP = "12345678",
                        Logradouro = "Rua Teste",
                        Numero = "123",
                        Bairro = "Bairro Teste",
                        Cidade = "Cidade Teste",
                        Estado = "SP"
                    }
                }
            };

            // Configurar o mock para simular a adição do cliente
            _clienteRepositoryMock.Setup(r => r.DocumentoExisteAsync(command.Documento, It.IsAny<Guid?>()))
                .ReturnsAsync(false);

            _clienteRepositoryMock.Setup(r => r.EmailExisteAsync(command.Email, It.IsAny<Guid?>()))
                .ReturnsAsync(false);

            _clienteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Cliente>()))
                .Callback<Domain.Entities.Cliente>(cliente => 
                {
                    // Não podemos definir o ID usando reflection porque o setter é protegido
                    // Vamos usar o ID que já foi gerado no construtor da entidade
                })
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(true);

            // Configurar o EventStore para aceitar qualquer evento
            _eventStoreMock.Setup(e => e.SaveEventAsync(It.IsAny<ClienteCriadoEvent>(), It.IsAny<Guid>(), "Cliente", 1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty(); // Verificamos apenas que o ID não é vazio, pois não podemos prever o valor exato
            _clienteRepositoryMock.Verify(r => r.DocumentoExisteAsync(command.Documento, It.IsAny<Guid?>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.EmailExisteAsync(command.Email, It.IsAny<Guid?>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Cliente>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _eventStoreMock.Verify(e => e.SaveEventAsync(It.IsAny<ClienteCriadoEvent>(), It.IsAny<Guid>(), "Cliente", 1), Times.Once);
        }

        [Theory]
        [InlineData("DocumentoExistente", "CPF/CNPJ já cadastrado")]
        [InlineData("EmailExistente", "E-mail já cadastrado")]
        [InlineData("PessoaFisicaMenorDeIdade", "Cliente deve ter pelo menos 18 anos")]
        [InlineData("PessoaJuridicaSemIE", "Inscrição Estadual é obrigatória")]
        public async Task Handle_ClienteInvalido_DeveLancarExcecao(string cenarioInvalido, string mensagemEsperada)
        {
            // Arrange
            var command = new CreateClienteCommand
            {
                TipoPessoa = cenarioInvalido == "PessoaJuridicaSemIE" ? 'J' : 'F',
                Nome = "Teste",
                Documento = "12345678900",
                IE = cenarioInvalido == "PessoaJuridicaSemIE" ? null : null,
                IsIsentoIE = cenarioInvalido == "PessoaJuridicaSemIE" ? false : false,
                DataNascimento = cenarioInvalido == "PessoaFisicaMenorDeIdade" ? DateTime.Now.AddYears(-16) : DateTime.Now.AddYears(-30),
                Telefone = "11999999999",
                Email = "teste@teste.com"
            };

            if (cenarioInvalido == "DocumentoExistente")
            {
                _clienteRepositoryMock.Setup(r => r.DocumentoExisteAsync(command.Documento, It.IsAny<Guid?>()))
                    .ReturnsAsync(true);
            }
            else if (cenarioInvalido == "EmailExistente")
            {
                _clienteRepositoryMock.Setup(r => r.DocumentoExisteAsync(command.Documento, It.IsAny<Guid?>()))
                    .ReturnsAsync(false);
                    
                _clienteRepositoryMock.Setup(r => r.EmailExisteAsync(command.Email, It.IsAny<Guid?>()))
                    .ReturnsAsync(true);
            }

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));
                
            exception.Message.Should().Contain(mensagemEsperada);
            
            if (cenarioInvalido == "DocumentoExistente")
            {
                _clienteRepositoryMock.Verify(r => r.DocumentoExisteAsync(command.Documento, It.IsAny<Guid?>()), Times.Once);
                _clienteRepositoryMock.Verify(r => r.EmailExisteAsync(command.Email, It.IsAny<Guid?>()), Times.Never);
            }
            else if (cenarioInvalido == "EmailExistente")
            {
                _clienteRepositoryMock.Verify(r => r.DocumentoExisteAsync(command.Documento, It.IsAny<Guid?>()), Times.Once);
                _clienteRepositoryMock.Verify(r => r.EmailExisteAsync(command.Email, It.IsAny<Guid?>()), Times.Once);
            }
            
            _clienteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Cliente>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
