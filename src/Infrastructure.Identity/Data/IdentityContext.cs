#pragma warning disable CS8618
using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

//[assembly: InternalsVisibleTo("WebAPITests")]

namespace Infrastructure.Identity.Data
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _configuration;
        private readonly IUserResolverService _userResolverService;

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public IdentityContext(DbContextOptions<IdentityContext> dbContextOptions, IConfiguration configuration, IUserResolverService userResolverService) : base(dbContextOptions)
        {
            _configuration = configuration;
            _userResolverService = userResolverService;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("IdentityCS"));
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RefreshToken>(entityBuilder =>
            {
                entityBuilder.Property(entity => entity.Id)
                        .ValueGeneratedOnAdd();
            });

            builder.Entity<IdentityRole>(entityBuilder =>
            {
                entityBuilder.Property(entity => entity.Id)
                        .ValueGeneratedOnAdd();
            });

            builder.Entity<ApplicationUser>(entityBuilder =>
            {
                entityBuilder.HasOne(entity => entity.RefreshToken)
                        .WithOne(entity => entity.ApplicationUser)
                        .HasForeignKey<ApplicationUser>(entity => entity.RefreshTokenId);
            });

            builder.SeedDefaultRoles();
            builder.SeedDefaultUsers();

            base.OnModelCreating(builder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is IAuditableEntity && (e.State == EntityState.Modified || e.State == EntityState.Added));

            if (!entries.Any())
            {
                return base.SaveChangesAsync(cancellationToken);
            }

            var userName = _userResolverService.GetUserName();

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
