using Microsoft.EntityFrameworkCore.Storage;
using StudentManagement.Core.Interfaces;
using System.Threading.Tasks;

namespace StudentManagement.Infrastructure.Data
{
    public class EfTransactionScope : ITransactionScope
    {
        private readonly IDbContextTransaction _transaction;

        public EfTransactionScope(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
} 