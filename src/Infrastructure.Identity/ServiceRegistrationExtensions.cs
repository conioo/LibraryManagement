﻿using Application.Interfaces;
using Application.Services;
using Infrastructure.Identity.Data;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Application
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddInfrastructureIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityContext>();

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();

            services.AddScoped<IJwtService, JwtService>();

            services.AddScoped<IUserResolverService, UserResolverService>();

            services.AddTransient<UserResolverService>(); // tutaj cos nie gra

            services.AddScoped<SignInManager<ApplicationUser>>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = configuration.GetSection("TokenValidationParameters").Get<TokenValidationParameters>();
                config.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Jwt").GetValue<string>("IssuerSigningKey")));
                config.TokenValidationParameters.ValidIssuer = configuration.GetSection("Jwt").GetValue<string>("Issuer");
                config.TokenValidationParameters.ValidAudience = configuration.GetSection("Jwt").GetValue<string>("Audience");
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                //bind
                options.Password = (PasswordOptions)configuration.GetSection("Accounts").GetSection("Password").Get(typeof(PasswordOptions));

                options.SignIn = new SignInOptions
                {
                    RequireConfirmedAccount = false,
                    RequireConfirmedEmail = true,
                    RequireConfirmedPhoneNumber = false,
                };

                options.User = new UserOptions
                {
                    RequireUniqueEmail = true,
                    AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"
                };
            }).AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();

            return services;
        }
    }
}