using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Entities;
using ClienteCadastro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClienteCadastro.Infrastructure.Data
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(ClienteDbContext context) : base(context) { }

        public async Task<Cliente?> GetByDocumentoAsync(string documento)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Documento == documento);
        }

        public async Task<Cliente?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> DocumentoExisteAsync(string documento, Guid? clienteId = null)
        {
            if (clienteId.HasValue)
            {
                return await _dbSet.AnyAsync(c => c.Documento == documento && c.Id != clienteId.Value);
            }
            
            return await _dbSet.AnyAsync(c => c.Documento == documento);
        }

        public async Task<bool> EmailExisteAsync(string email, Guid? clienteId = null)
        {
            if (clienteId.HasValue)
            {
                return await _dbSet.AnyAsync(c => c.Email == email && c.Id != clienteId.Value);
            }
            
            return await _dbSet.AnyAsync(c => c.Email == email);
        }

        public async Task<Cliente?> GetClienteComEnderecosAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.Enderecos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<(IEnumerable<Cliente> Data, int Total)> GetAllSearchAsync(string searchTerm, int pageNumber, int pageSize)
        {
            // Se o termo de busca estiver vazio, retorna todos os clientes paginados
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetPagedAsync(pageNumber, pageSize);
            }

            // Normaliza o termo de busca
            searchTerm = searchTerm.Trim().ToLower();

            // Consulta para buscar por nome ou documento (CPF/CNPJ)
            var query = _dbSet.Where(c => 
                c.Nome.ToLower().Contains(searchTerm) || 
                c.Documento.ToLower().Contains(searchTerm));

            var total = await query.CountAsync();
            var data = await query
                .Include(c => c.Enderecos)
                .OrderBy(c => c.Nome)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public override async Task<(IEnumerable<Cliente> Data, int Total)> GetPagedAsync(int pageNumber, int pageSize, 
            System.Linq.Expressions.Expression<Func<Cliente, bool>>? filter = null)
        {
            var query = filter != null ? _dbSet.Where(filter) : _dbSet;
            
            var total = await query.CountAsync();
            var data = await query
                .Include(c => c.Enderecos)
                .OrderBy(c => c.Nome)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return (data, total);
        }
    }
}
