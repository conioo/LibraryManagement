using Application;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Serilog;
using WebAPI.Conventions;
using WebAPI.Extensions;
using WebAPI.Installers.Extensions;
using WebAPI.Middleware;

namespace WebAPI
{
    public class Program
    {
        internal static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

            builder.Services.AddControllers(options =>
            {
                options.Conventions.Add(new RoutePrefixConvention(builder.Configuration));
            });

            builder.Services.InstallServicesInAssembly(builder.Configuration);
            builder.Services.AddApplication();
            builder.Services.AddInfrastructurePersistence();
            builder.Services.AddInfrastructureIdentity(builder.Configuration);

            //builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            app.UseReactiveComponents();

            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
                    }
                });
            }
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseMiddleware<TokenManagerMiddleware>();

            app.MapControllers();
            app.UseAuthorization();

            app.Run();
        }
    }
}