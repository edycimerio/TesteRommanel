using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteCadastro.Application.DTOs;

namespace ClienteCadastro.Application.Interfaces
{
    public interface IClienteService
    {
        Task<Guid> CreateAsync(ClienteDTO clienteDto);
        Task<bool> UpdateAsync(ClienteDTO clienteDto);
        Task<bool> DeleteAsync(Guid id);
        Task<ClienteDTO> GetByIdAsync(Guid id);
        Task<(IEnumerable<ClienteDTO> Data, int Total)> GetPagedAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<ClienteDTO> Data, int Total)> SearchAsync(string searchTerm, int pageNumber, int pageSize);
    }
}
