using WebAPI.Configurations;
using WebAPI.Installers.Interfaces;

namespace WebAPI.Installers
{
    public class SwaggerInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // var applicationSection = configuration.GetSection("Application");

            services.ConfigureOptions<ConfigureSwaggerOptions>();
            services.AddSwaggerGen();

            //services.AddSwaggerGen(config =>
            //{
            //    config.SwaggerDoc("v1", new OpenApiInfo
            //    {
            //        Version = applicationSection.GetValue<string>("Version"),
            //        Title = applicationSection.GetValue<string>("Title"),
            //        Description = applicationSection.GetValue<string>("Description"),
            //        Contact = applicationSection.GetSection("Contact").Get<OpenApiContact>(),
            //        License = applicationSection.GetSection("License").Get<OpenApiLicense>()
            //    });

            //    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //    {
            //        Description = @"JWT Authorization header using the Bearer scheme. 
            //                      Enter 'Bearer' [space] and then your token in the text input below.
            //                      Example: 'Bearer {token}'",
            //        Name = "Authorization",
            //        In = ParameterLocation.Header,
            //        Type = SecuritySchemeType.ApiKey,
            //        Scheme = "Bearer"
            //    });

            //    config.AddSecurityRequirement(new OpenApiSecurityRequirement()
            //    {
            //        {
            //            new OpenApiSecurityScheme()
            //            {
            //                Name = "Bearer",
            //                In = ParameterLocation.Header,
            //                Reference = new OpenApiReference()
            //                {
            //                    Type = ReferenceType.SecurityScheme,
            //                    Id = "Bearer"
            //                }
            //            },
            //            new List<string>()
            //        }
            //    });

            //    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            //    config.EnableAnnotations();
            //    config.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            //});
        }
    }
}