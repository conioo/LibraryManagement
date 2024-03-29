﻿using Domain.Settings;
using Microsoft.Extensions.Options;
using Sieve.Models;
using WebAPI.Installers.Interfaces;

namespace WebAPI.Installers
{
    public class OptionsInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var defaultMailSection = configuration.GetSection("Mail").GetSection("Default").Value;

            services.AddOptions<MailSettings>().Bind(configuration.GetSection("Mail").GetSection(defaultMailSection));
            services.AddOptions<JwtSettings>().Bind(configuration.GetSection("Jwt"));
            services.AddOptions<ApplicationSettings>().Bind(configuration.GetSection("Application"));

            services.AddOptions<SieveOptions>().Bind(configuration.GetSection("Sieve"));
            services.AddOptions<UserNameSettings>().Bind(configuration.GetSection("Accounts").GetSection("UserName"));

            services.AddOptions<ReservationSettings>().Bind(configuration.GetSection("Application").GetSection("Reservation"));
            services.AddOptions<RentalSettings>().Bind(configuration.GetSection("Application").GetSection("Rental"));

            services.AddOptions<PathSettings>().Bind(configuration.GetSection("Application").GetSection("Paths"));
        }
    }
}