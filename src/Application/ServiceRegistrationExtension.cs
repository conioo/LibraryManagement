using Application.Interfaces;
using Application.Reactive.Interfaces;
using Application.Reactive.Observers;
using Application.Services;
using Application.Sieve;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace Application
{
    public static class ServiceRegistrationExtension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSingleton<IMailTransport, SmtpClient>();

            services.AddSingleton<IEndOfReservation, EndOfReservation>();
            services.AddSingleton<ICountingOfPenaltyCharges, CountingOfPenaltyCharges>();

            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ILibraryService, LibraryService>();
            services.AddScoped<ICopyService, CopyService>();
            services.AddScoped<IProfileService, ProfileService>();

            services.AddScoped<IRentalService, RentalService>();

            services.AddSingleton<IEmailService, EmailService>();


            services.AddHttpContextAccessor();

            services.AddDistributedMemoryCache();
            return services;
        }
    }
}