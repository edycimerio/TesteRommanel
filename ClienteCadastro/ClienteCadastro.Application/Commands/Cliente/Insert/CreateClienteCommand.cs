using System;
using System.Collections.Generic;
using ClienteCadastro.Application.DTOs;
using MediatR;

namespace ClienteCadastro.Application.Commands.Cliente.Insert
{
    public class CreateClienteCommand : IRequest<Guid>
    {
        public char TipoPessoa { get; set; } // 'F' para física, 'J' para jurídica
        public string Nome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string? IE { get; set; }
        public bool IsIsentoIE { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<EnderecoDTO> Enderecos { get; set; } = new List<EnderecoDTO>();
    }
}
