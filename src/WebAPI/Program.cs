using Application;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebAPI.Conventions;
using WebAPI.Extensions;
using WebAPI.Installers.Extensions;
using WebAPI.Middleware;

//usunas
//public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
//{
//    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        return DateOnly.FromDateTime(reader.GetDateTime());
//    }

//    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
//    {
//        var isoDate = value.ToString("O");
//        writer.WriteStringValue(isoDate);
//    }
//}

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
            //    .AddJsonOptions(options =>
            //{
            //    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            //}); ;

            builder.Services.InstallServicesInAssembly(builder.Configuration);

            builder.Services.AddApplication();
            builder.Services.AddInfrastructurePersistence();
            builder.Services.AddInfrastructureIdentity(builder.Configuration);

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