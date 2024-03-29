﻿using Application.Dtos;
using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Interfaces;
using Application.Reactive.Interfaces;
using CommonContext;
using Domain.Interfaces;
using Domain.Settings;
using Infrastructure.Identity.Data;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Interfaces;
using Infrastructure.Identity.Roles;
using Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Http.Json;
using WebAPI;
using WebAPI.ApiRoutes;
using Profile = Domain.Entities.Profile;

namespace WebAPITests.Integration
{
    public class SharedContext : WebApplicationFactory<Program>
    {
        public ApplicationSettings ApplicationSettings { get; private set; }
        public IUnitOfWork DbContext { get; private set; }
        public IdentityContext IdentityDbContext { get; private set; }
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public JwtSettings JwtSettings { get; private set; }
        public IJwtService JwtService { get; private set; }
        public ApplicationUser? DefaultUser { get; private set; }
        public Profile? DefaultProfile { get; private set; }
        public IDistributedCache Cache { get; private set; }

        public IConfiguration Configuration;

        private Dictionary<Type, object> Mocks { get; set; } = new Dictionary<Type, object>();
        private IServiceScopeFactory _scopeFactory;
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
            Cache = serviceProvider.GetRequiredService<IDistributedCache>();

            IdentityDbContext.Database.EnsureCreated();
            DbContext.Database.EnsureCreated();

            if (String.IsNullOrEmpty(_options.controllerPrefix))
            {
                ClientOptions.BaseAddress = new Uri($"{ApplicationSettings.BaseAddress}/{ApplicationSettings.RoutePrefix}/");
            }
            else
            {
                ClientOptions.BaseAddress = new Uri($"{ApplicationSettings.BaseAddress}/{ApplicationSettings.RoutePrefix}/{_options.controllerPrefix}/");
            }

            if (_options.addDefaultUser is true)
            {
                DefaultUser = GetDefaultUser();

                if (_options.addProfileForDefaultUser)
                {
                    DefaultProfile = GetProfile(DefaultUser);
                }
            }
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

            // add another
        }

        public void ResetState()
        {
            RefreshScope();

            DbContext.Database.EnsureDeleted();
            DbContext.Database.EnsureCreated();

            IdentityDbContext.Database.EnsureDeleted();
            IdentityDbContext.Database.EnsureCreated();

            if (_options.addCountingOfPenaltyChargesMock)
            {
                RefreshMock<ICountingOfPenaltyCharges>();
            }

            if (_options.addEmailServiceMock)
            {
                RefreshMock<IEmailService>();
            }

            if (_options.addEndOfReservationMock)
            {
                RefreshMock<IEndOfReservation>();
            }

            if (_options.addFilesServiceMock)
            {
                RefreshMock<IFilesService>();
            }

            if (_options.addDefaultUser)
            {
                DefaultUser = GetDefaultUser();

                if (_options.addProfileForDefaultUser)
                {
                    DefaultProfile = GetProfile(DefaultUser);
                }
            }

            IdentityDbContext.SaveChanges();
        }

        public void RefreshMock<T>() where T : class
        {
            var service = GetMock<T>();
            service.Invocations.Clear();
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
                //builder.Sources.Clear();

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

                services.AddDbContext<IUnitOfWork, LibraryDbContext>(options =>
                {
                    options.EnableSensitiveDataLogging(true);
                    options.UseInMemoryDatabase("inMemoryDb" + _options.controllerPrefix);
                });
                services.AddDbContext<IdentityContext>(options =>
                {
                    options.EnableSensitiveDataLogging();
                    options.UseInMemoryDatabase("inMemoryDb-Identity" + _options.controllerPrefix);
                });

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

                if (_options.addCountingOfPenaltyChargesMock)
                {
                    var countingOfPenaltyCharges = services.Single(service => service.ServiceType == typeof(ICountingOfPenaltyCharges));

                    services.Remove(countingOfPenaltyCharges);

                    var countingOfPenaltyChargesMock = new Mock<ICountingOfPenaltyCharges>();

                    Mocks[typeof(ICountingOfPenaltyCharges)] = countingOfPenaltyChargesMock;

                    services.AddSingleton(countingOfPenaltyChargesMock.Object);
                }

                if (_options.addEndOfReservationMock)
                {
                    var endOfReservation = services.Single(service => service.ServiceType == typeof(IEndOfReservation));

                    services.Remove(endOfReservation);

                    var endOfReservationMock = new Mock<IEndOfReservation>();

                    Mocks[typeof(IEndOfReservation)] = endOfReservationMock;

                    services.AddSingleton(endOfReservationMock.Object);
                }

                if (_options.addFilesServiceMock)
                {
                    var filesService = services.Single(service => service.ServiceType == typeof(IFilesService));

                    services.Remove(filesService);

                    var filesServiceMock = new Mock<IFilesService>();

                    filesServiceMock.Setup(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>())).Returns((ICollection<IFormFile> list) =>
                    {
                        var fileNames = new List<string>();

                        for (int i = 0; i < list.Count; ++i)
                        {
                            fileNames.Add("C:\\mock.png");
                        }

                        return Task.FromResult((ICollection<string>)fileNames);
                    });

                    Mocks[typeof(IFilesService)] = filesServiceMock;

                    services.AddSingleton(filesServiceMock.Object);
                }
            });
        }

        public async Task<LoginResponse> LoginAsync(HttpClient client, ApplicationUser user, string password)
        {
            var loginRequest = new LoginRequest();

            loginRequest.Email = user.Email;
            loginRequest.Password = password;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(client.BaseAddress + Accounts.Prefix + "/" + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await client.SendAsync(request);

            var responseLogin = await response.Content.ReadFromJsonAsync<LoginResponse>();

            return responseLogin;
        }

        private ApplicationUser GetDefaultUser()
        {
            var defaultUser = DataGenerator.Get<ApplicationUser>(1).First();
            defaultUser.UserName = "default";

            UserManager.CreateAsync(defaultUser, DataGenerator.GetUserPassword);

            return defaultUser;
        }
        private Profile GetProfile(ApplicationUser user)
        {
            Profile profile;

            if (_options.removeRentalsAndReservationsForTheProfile)
            {
                profile = DataGenerator.GetOne<Profile>();
            }
            else
            {
                profile = DataGenerator.GetOneWithDependencies<Profile>();
            }

            profile.IsActive = _options.isActiveProfile;
            profile.UserId = user.Id;

            DbContext.Set<Profile>().Add(profile);
            DbContext.SaveChangesAsync().Wait();

            user.ProfileCardNumber = profile.LibraryCardNumber;

            IdentityDbContext.Users.Update(user);
            IdentityDbContext.SaveChanges();

            return profile;
        }
        public async Task<ApplicationUser> GetBasicConfirmUser()
        {
            var user = DataGenerator.Get<ApplicationUser>(1).First();

            await UserManager.CreateAsync(user, DataGenerator.GetUserPassword);

            var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);

            await UserManager.AddToRoleAsync(user, UserRoles.Basic);
            await UserManager.ConfirmEmailAsync(user, token);

            RefreshIdentityDb();

            return user;
        }
    }
}