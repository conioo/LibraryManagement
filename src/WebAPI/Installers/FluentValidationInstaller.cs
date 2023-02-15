using Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using System.Reflection;
using WebAPI.Installers.Interfaces;

namespace WebAPI.Installers
{
    public class FluentValidationInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            //services.AddFluentValidationAutoValidation();
            services.AddFluentValidationAutoValidation(options => options.ImplicitlyValidateRootCollectionElements = true);

            services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(RegisterRequestValidator)));

            services.AddFluentValidationClientsideAdapters();
            services.AddFluentValidationRulesToSwagger();
        }

    }
}
