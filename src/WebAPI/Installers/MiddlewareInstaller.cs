using WebAPI.Installers.Interfaces;
using WebAPI.Middleware;

namespace WebAPI.Installers
{
    public class MiddlewareInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ErrorHandlingMiddleware>();
            services.AddScoped<TokenManagerMiddleware>();//transient
        }
    }
}
