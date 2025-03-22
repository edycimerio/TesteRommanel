using System;
using System.Collections.Generic;
using System.Linq;

namespace ClienteCadastro.Domain.Entities
{
    public class Cliente : Entity
    {
        public char TipoPessoa { get; private set; } // 'F' para física, 'J' para jurídica
        public string Nome { get; private set; } = string.Empty;
        public string Documento { get; private set; } = string.Empty;
        public string IE { get; private set; } = string.Empty;
        public bool IsIsentoIE { get; private set; }
        public DateTime? DataNascimento { get; private set; }
        public string Telefone { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        
        private readonly List<Endereco> _enderecos;
        public IReadOnlyCollection<Endereco> Enderecos => _enderecos;

        // Construtor protegido para o EF Core
        protected Cliente() : base()
        {
            _enderecos = new List<Endereco>();
        }

        // Construtor para Pessoa Física
        public Cliente(string nome, string cpf, DateTime dataNascimento, string telefone, string email) : this()
        {
            TipoPessoa = 'F';
            Nome = nome;
            Documento = cpf;
            DataNascimento = dataNascimento;
            Telefone = telefone;
            Email = email;
            Ativo = true;
        }

        // Construtor para Pessoa Jurídica
        public Cliente(string razaoSocial, string cnpj, string ie, bool isIsentoIE, DateTime dataFundacao, string telefone, string email) : this()
        {
            TipoPessoa = 'J';
            Nome = razaoSocial;
            Documento = cnpj;
            IE = ie;
            IsIsentoIE = isIsentoIE;
            DataNascimento = dataFundacao;
            Telefone = telefone;
            Email = email;
            Ativo = true;
        }

        public void AtualizarPessoaFisica(string nome, DateTime dataNascimento, string telefone, string email)
        {
            if (TipoPessoa != 'F')
                throw new InvalidOperationException("Este cliente não é uma pessoa física.");

            Nome = nome;
            DataNascimento = dataNascimento;
            Telefone = telefone;
            Email = email;
            AtualizarDataModificacao();
        }

        public void AtualizarPessoaJuridica(string razaoSocial, string ie, bool isIsentoIE, DateTime dataFundacao, string telefone, string email)
        {
            if (TipoPessoa != 'J')
                throw new InvalidOperationException("Este cliente não é uma pessoa jurídica.");

            Nome = razaoSocial;
            IE = ie;
            IsIsentoIE = isIsentoIE;
            DataNascimento = dataFundacao;
            Telefone = telefone;
            Email = email;
            AtualizarDataModificacao();
        }

        public void AdicionarEndereco(Endereco endereco)
        {
            _enderecos.Add(endereco);
            AtualizarDataModificacao();
        }

        public void RemoverEndereco(Guid enderecoId)
        {
            var endereco = _enderecos.FirstOrDefault(e => e.Id == enderecoId);
            if (endereco != null)
            {
                _enderecos.Remove(endereco);
                AtualizarDataModificacao();
            }
        }

        public void Ativar()
        {
            Ativo = true;
            AtualizarDataModificacao();
        }

        public void Desativar()
        {
            Ativo = false;
            AtualizarDataModificacao();
        }

        // Validações específicas
        public bool EhMaiorDeIdade()
        {
            if (TipoPessoa != 'F' || !DataNascimento.HasValue)
                return true;

            return DataNascimento.Value.AddYears(18) <= DateTime.Today;
        }

        public bool PossuiIEValida()
        {
            if (TipoPessoa != 'J')
                return true;

            return !string.IsNullOrEmpty(IE) || IsIsentoIE;
        }
    }
}
