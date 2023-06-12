#pragma warning disable CS8618
using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Seeds;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity.Data
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>, IUnitOfWork
    {
        private readonly IConfiguration _configuration;
        private readonly IUserResolverService _userResolverService;
        private readonly ILogger<IdentityContext> _logger;

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public IdentityContext(DbContextOptions<IdentityContext> dbContextOptions, IConfiguration configuration, IUserResolverService userResolverService, ILogger<IdentityContext> logger) : base(dbContextOptions)
        {
            _configuration = configuration;
            _userResolverService = userResolverService;
            _logger = logger;
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

            builder.Entity<ApplicationUser>(entityBuilder =>
            {
                entityBuilder.HasOne(entity => entity.RefreshToken)
                        .WithOne(entity => entity.ApplicationUser)
                        .HasForeignKey<ApplicationUser>(entity => entity.RefreshTokenId);
            });

            builder.SeedDefaultRoles();
            _logger.LogDebug("roles seeded");

            builder.SeedDefaultUsers();
            _logger.LogDebug("users seeded");

            base.OnModelCreating(builder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is IAuditableEntity && (e.State == EntityState.Modified || e.State == EntityState.Added));

            if (!entries.Any())
            {
                return base.SaveChangesAsync(cancellationToken);
            }

            var userName = _userResolverService.GetUserName;
            
            DateTime now;

            foreach (var entityEntry in entries)
            {
                now = DateTime.Now;

                var auditable = (IAuditableEntity)entityEntry.Entity;

                auditable.LastModifiedBy = userName;
                auditable.LastModified = now;

                if (entityEntry.State == EntityState.Added)
                {
                    auditable.CreatedBy = userName;
                    auditable.Created = now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            return SaveChangesAsync().Result;
        }
    }
}
