using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClienteCadastro.Infrastructure.Data
{
    public class EnderecoRepository : Repository<Endereco>, IEnderecoRepository
    {
        public EnderecoRepository(ClienteDbContext context) : base(context) { }

        public async Task<IEnumerable<Endereco>> GetByClienteIdAsync(Guid clienteId)
        {
            return await _dbSet
                .Where(e => e.ClienteId == clienteId)
                .ToListAsync();
        }
    }
}
