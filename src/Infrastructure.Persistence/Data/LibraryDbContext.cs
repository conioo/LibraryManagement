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
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<CopyHistory> CopyHistories { get; set; }
        public DbSet<ProfileHistory> ProfilesHistories { get; set; }
        public DbSet<ArchivalRental> ArchivalRentals { get; set; }
        public DbSet<ArchivalReservation> ArchivalReservations { get; set; }

        public LibraryDbContext()
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
               // optionsBuilder.UseSqlServer("Server=(LocalDb)\\MSSQLLocalDB;Database=LibraryDB;Trusted_Connection=True;");
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

                entityBuilder.HasOne(copy => copy.CurrentRental)
                .WithOne(rental => rental.Copy)
                .HasForeignKey<Copy>(copy => copy.CurrentRentalId);

                entityBuilder.HasOne(copy => copy.CurrentReservation)
               .WithOne(reservation => reservation.Copy)
               .HasForeignKey<Copy>(copy => copy.CurrentReservationId);
            });

            builder.Entity<Profile>(entityBuilder =>
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

            builder.Entity<CopyHistory>(entityBuilder =>
            {
                entityBuilder.Property(entity => entity.Id).ValueGeneratedOnAdd();
            });

            builder.Entity<ProfileHistory>(entityBuilder =>
            {
                entityBuilder.Property(entity => entity.Id).ValueGeneratedOnAdd();
            });

            builder.Entity<ArchivalRental>(entityBuilder =>
            {
                entityBuilder.Property(entity => entity.Id).ValueGeneratedOnAdd();
            });

            builder.Entity<ArchivalReservation>(entityBuilder =>
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
    }
}