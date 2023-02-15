using Application.Interfaces;
using Application.Services;
using Application.Sieve;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace Application
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //services.AddScoped<IMailTransport, SmtpClient>();
            services.AddSingleton<IMailTransport, SmtpClient>();

            services.AddScoped<IItemService, ItemService>();

            // services.AddScoped<IEmailService, EmailService>();

            services.AddSingleton<IEmailService, EmailService>();


            services.AddHttpContextAccessor();

            services.AddDistributedMemoryCache();
            return services;
        }
    }
}