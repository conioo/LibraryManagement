using Domain.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence
{
    public static class ServiceRegistrationExtension
    {
        public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection services)
        {
            services.AddDbContext<IUnitOfWork, LibraryDbContext>();

            return services;
        }
    }
}
