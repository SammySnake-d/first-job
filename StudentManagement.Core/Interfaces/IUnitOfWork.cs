using System.Data;
using System.Threading.Tasks;

namespace StudentManagement.Core.Interfaces
{
    public interface IUnitOfWork
    {
        Task<ITransactionScope> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }

    public interface ITransactionScope : System.IDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
} 