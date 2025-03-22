using System;
using MediatR;

namespace ClienteCadastro.Application.Commands.Endereco.Update
{
    public class UpdateEnderecoCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string CEP { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
