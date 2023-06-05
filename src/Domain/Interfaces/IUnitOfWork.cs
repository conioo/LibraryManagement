using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Domain.Interfaces
{
    public interface IUnitOfWork
    {
        public DbSet<TEntity> Set<TEntity>() where TEntity : class;// where TEntity : BaseEntity;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        public int SaveChanges();
        public DatabaseFacade Database { get; }
    }
}