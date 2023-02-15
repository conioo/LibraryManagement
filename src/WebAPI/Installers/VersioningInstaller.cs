using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using WebAPI.Installers.Interfaces;

namespace WebAPI.Installers
{
    public class VersioningInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = ApiVersion.Parse(configuration.GetSection("Application").GetValue<string>("Version"));
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'version:' VVV";
            });

        }
    }
}
