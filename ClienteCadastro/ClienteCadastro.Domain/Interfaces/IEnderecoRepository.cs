using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Entities;

namespace ClienteCadastro.Domain.Interfaces
{
    public interface IEnderecoRepository : IRepository<Endereco>
    {
        Task<IEnumerable<Endereco>> GetByClienteIdAsync(Guid clienteId);
    }
}
