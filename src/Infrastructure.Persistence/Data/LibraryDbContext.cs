#pragma warning disable CS8618
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.Data
{
    public class LibraryDbContext : DbContext, IUnitOfWork
    {
        private readonly IConfiguration _configuration;
        private readonly IUserResolverService _userResolverService;
        public DbSet<Item> Items { get; set; }

        public DbSet<Copy> Copies { get; set; }
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Profil> Profiles { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public LibraryDbContext(DbContextOptions<LibraryDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }
        public LibraryDbContext(DbContextOptions<LibraryDbContext> dbContextOptions, IConfiguration configuration, IUserResolverService userResolverService) : base(dbContextOptions)
        {
            _configuration = configuration;
            _userResolverService = userResolverService;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("LibraryCS"));
            }
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Item>(entityBuilder =>
            {
                entityBuilder.Property(entity => entity.Id).ValueGeneratedOnAdd();
                entityBuilder.Property(entity => entity.Title).HasMaxLength(50);

            });

            builder.Entity<Library>(entityBuilder =>
            {
                entityBuilder.Property(entity => entity.Id).ValueGeneratedOnAdd();
            });

            builder.Entity<Copy>(entityBuilder =>
            {
                entityBuilder.HasKey(entity => entity.InventoryNumber);

                entityBuilder.Property(entity => entity.InventoryNumber).ValueGeneratedOnAdd();
            });

            builder.Entity<Profil>(entityBuilder =>
            {
                entityBuilder.HasKey(entity => entity.LibraryCardNumber);

                entityBuilder.Property(entity => entity.LibraryCardNumber).ValueGeneratedOnAdd();
            });

            builder.Entity<Rental>(entityBuilder =>
            {
                entityBuilder.Property(entity => entity.Id).ValueGeneratedOnAdd();
            });

            builder.Entity<Reservation>(entityBuilder =>
            {
                entityBuilder.Property(entity => entity.Id).ValueGeneratedOnAdd();
            });

            base.OnModelCreating(builder);
        }
        DbSet<TEntity> IUnitOfWork.Set<TEntity>()
        {
            return Set<TEntity>();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is IAuditableEntity && (e.State == EntityState.Modified || e.State == EntityState.Added));

            if (!entries.Any())
            {
                return base.SaveChangesAsync(cancellationToken);
            }

            var userName = _userResolverService.GetUserName;

            DateTime UtcNow;

            foreach (var entityEntry in entries)
            {
                UtcNow = DateTime.UtcNow;

                var auditable = (IAuditableEntity)entityEntry.Entity;

                auditable.LastModifiedBy = userName;
                auditable.LastModified = UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    auditable.CreatedBy = userName;
                    auditable.Created = UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}