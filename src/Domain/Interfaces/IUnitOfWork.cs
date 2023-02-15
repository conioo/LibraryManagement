using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Domain.Interfaces
{
    public interface IUnitOfWork
    {
        public DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        public DatabaseFacade Database { get; }
    }
}