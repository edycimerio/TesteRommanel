using System;
using System.Collections.Generic;

namespace ClienteCadastro.Application.DTOs
{
    public class ClienteDTO
    {
        public Guid Id { get; set; }
        public char TipoPessoa { get; set; } // 'F' para física, 'J' para jurídica
        public string Nome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string? IE { get; set; }
        public bool IsIsentoIE { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public bool Ativo { get; set; }
        public List<EnderecoDTO> Enderecos { get; set; } = new List<EnderecoDTO>();
    }
}
