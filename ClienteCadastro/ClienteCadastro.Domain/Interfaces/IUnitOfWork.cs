using System;
using System.Threading.Tasks;

namespace ClienteCadastro.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IClienteRepository ClienteRepository { get; }
        IEnderecoRepository EnderecoRepository { get; }
        
        Task<bool> CommitAsync();
        Task RollbackAsync();
        Task BeginTransactionAsync();
    }
}
