using System;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClienteCadastro.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClienteDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public IClienteRepository ClienteRepository { get; }
        public IEnderecoRepository EnderecoRepository { get; }

        public UnitOfWork(ClienteDbContext context, 
                         IClienteRepository clienteRepository,
                         IEnderecoRepository enderecoRepository)
        {
            _context = context;
            ClienteRepository = clienteRepository;
            EnderecoRepository = enderecoRepository;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task<bool> CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
                
                return true;
            }
            catch
            {
                await RollbackAsync();
                return false;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
