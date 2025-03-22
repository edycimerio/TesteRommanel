using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ClienteCadastro.Domain.Entities;

namespace ClienteCadastro.Domain.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> GetByDocumentoAsync(string documento);
        Task<Cliente?> GetByEmailAsync(string email);
        Task<bool> DocumentoExisteAsync(string documento, Guid? clienteId = null);
        Task<bool> EmailExisteAsync(string email, Guid? clienteId = null);
        Task<Cliente?> GetClienteComEnderecosAsync(Guid id);
        Task<(IEnumerable<Cliente> Data, int Total)> GetAllSearchAsync(string searchTerm, int pageNumber, int pageSize);
    }
}
