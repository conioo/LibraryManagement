﻿using Application.Interfaces;
using CommonContext;
using Domain.Interfaces;
using Domain.Settings;
using Infrastructure.Identity.Data;
using Infrastructure.Identity.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using WebAPI;

namespace WebAPITests.Integration
{
    public class SharedContext : WebApplicationFactory<Program>
    {
        public ApplicationSettings ApplicationSettings { get; set; }
        public IUnitOfWork DbContext { get; set; }
        public IdentityContext IdentityDbContext { get; set; }

        public UserManager<ApplicationUser> UserManager { get; set; }
        public JwtSettings JwtSettings { get; set; }
        public IJwtService JwtService { get; set; }

        private Dictionary<Type, object> Mocks { get; set; } = new Dictionary<Type, object>();

        private IServiceScopeFactory _scopeFactory;
        public IConfiguration Configuration;
        private SharedContextOptions _options;

        private static SharedContextOptions InvocationHelper(Action<SharedContextOptions> optionsBuilder)
        {
            var sharedContextOptions = new SharedContextOptions();

            optionsBuilder(sharedContextOptions);

            return sharedContextOptions;
        }

        public override ValueTask DisposeAsync()
        {
            return base.DisposeAsync();
        }

        public SharedContext() : this(new SharedContextOptions())
        {

        }

        public SharedContext(SharedContextOptions options)
        {
            _options = options;

            var serviceProvider = Services.CreateScope().ServiceProvider;

            ApplicationSettings = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
            JwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
            DbContext = serviceProvider.GetRequiredService<IUnitOfWork>();
            IdentityDbContext = serviceProvider.GetRequiredService<IdentityContext>();
            _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            JwtService = serviceProvider.GetRequiredService<IJwtService>();
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();

            IdentityDbContext.Database.EnsureCreated();
            DbContext.Database.EnsureCreated();

            ClientOptions.BaseAddress = new Uri($"{ApplicationSettings.BaseAddress}/{ApplicationSettings.RoutePrefix}/{_options.controllerPrefix}/");
        }

        public SharedContext(Action<SharedContextOptions> optionsBuilder) : this(InvocationHelper(optionsBuilder))
        {
        }

        public void RefreshDb()
        {
            DbContext = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        }
        public void RefreshIdentityDb()
        {
            IdentityDbContext = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();
        }
        public void RefreshScope()
        {
            var serviceProvider = _scopeFactory.CreateScope().ServiceProvider;

            DbContext = serviceProvider.GetRequiredService<IUnitOfWork>();
            IdentityDbContext = serviceProvider.GetRequiredService<IdentityContext>();
            UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            JwtService = serviceProvider.GetRequiredService<IJwtService>();
        }

        public Mock<T> GetMock<T>() where T : class
        {
            return (Mock<T>)Mocks[typeof(T)];
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureAppConfiguration((context, builder) =>
            {
                builder.Sources.Clear();

                var projectDir = Directory.GetCurrentDirectory();
                var configurationPath = Path.Combine(projectDir, "appsettings.test.json");

                builder.AddJsonFile(configurationPath);
            });

            builder.ConfigureServices(services =>
            {
                var dbContextOptions = services.Single(service => service.ServiceType == typeof(DbContextOptions<LibraryDbContext>));
                var identityDbContextOptions = services.Single(service => service.ServiceType == typeof(DbContextOptions<IdentityContext>));

                services.Remove(dbContextOptions);
                services.Remove(identityDbContextOptions);

                services.AddDbContext<IUnitOfWork, LibraryDbContext>(options => options.UseInMemoryDatabase("inMemoryDb" + _options.controllerPrefix));
                services.AddDbContext<IdentityContext>(options => options.UseInMemoryDatabase("inMemoryDb-Identity" + _options.controllerPrefix));

                if (_options.addFakePolicyEvaluator)
                {
                    var defaultPolicyEvaluator = services.Single(service => service.ServiceType == typeof(IPolicyEvaluator));
                    services.Remove(defaultPolicyEvaluator);

                    //services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                    services.AddScoped<IPolicyEvaluator, FakePolicyEvaluator>();
                }

                if (_options.addOldFakePolicyEvaluator)
                {
                    var defaultPolicyEvaluator = services.Single(service => service.ServiceType == typeof(IPolicyEvaluator));
                    services.Remove(defaultPolicyEvaluator);

                    services.AddScoped<IPolicyEvaluator, OldFakePolicyEvaluator>();
                }

                if (_options.addEmailServiceMock)
                {
                    var emailService = services.Single(service => service.ServiceType == typeof(IEmailService));

                    services.Remove(emailService);

                    var emailMock = new Mock<IEmailService>();

                    //emailMock.Setup(service => service.SendAsync(It.IsAny<Email>())).Returns((Email email) => email.To);

                    Mocks[typeof(IEmailService)] = emailMock;

                    services.AddSingleton(emailMock.Object);
                }
            });
        }
    }
}