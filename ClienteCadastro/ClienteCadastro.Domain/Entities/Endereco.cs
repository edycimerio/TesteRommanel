using System;

namespace ClienteCadastro.Domain.Entities
{
    public class Endereco : Entity
    {
        public Guid ClienteId { get; private set; }
        public string CEP { get; private set; } = string.Empty;
        public string Logradouro { get; private set; } = string.Empty;
        public string Numero { get; private set; } = string.Empty;
        public string? Complemento { get; private set; }
        public string Bairro { get; private set; } = string.Empty;
        public string Cidade { get; private set; } = string.Empty;
        public string Estado { get; private set; } = string.Empty;

        // Propriedade de navegação (para EF Core)
        public Cliente? Cliente { get; private set; }

        // Construtor protegido para o EF Core
        protected Endereco() : base() { }

        public Endereco(Guid clienteId, string cep, string logradouro, string numero, 
                        string bairro, string cidade, string estado, string? complemento = null) : base()
        {
            ClienteId = clienteId;
            CEP = cep;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
        }

        public void Atualizar(string cep, string logradouro, string numero, 
                             string bairro, string cidade, string estado, string? complemento = null)
        {
            CEP = cep;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            AtualizarDataModificacao();
        }
    }
}
