﻿using Application.Interfaces;
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

            services.AddSingleton<IMailTransport, SmtpClient>();

            services.AddScoped<IUserResolverService, UserResolverService>();

            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ILibraryService, LibraryService>();
            services.AddScoped<ICopyService, CopyService>();

            services.AddSingleton<IEmailService, EmailService>();


            services.AddHttpContextAccessor();

            services.AddDistributedMemoryCache();
            return services;
        }
    }
}