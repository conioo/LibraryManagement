using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Validators.Extension
{

    // usunac to nie potrzebne 
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddValidation(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddFluentValidation(options =>
            {
                options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                //RegisterValidatorsFromAssembly(Assembly.GetAssembly(typeof(RegisterRequestValidator)));
            });

            //serviceCollection.Services.AddScoped<IValidator<SieveModel>, SieveModelValidator>();

            //serviceCollection.AddFluentValidationAutoValidation();
            //serviceCollection.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            //serviceCollection.AddFluentValidationClientsideAdapters();
            //serviceCollection.AddFluentValidationRulesToSwagger();

            return serviceCollection;
        }
    }
}
