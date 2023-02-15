using Application.Interfaces;
using Application.Mappings;
using Application.Sieve;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Sieve.Models;
using Sieve.Services;

namespace ApplicationTests
{
    public class ContextFixture
    {
        private IConfiguration _configuration { get; set; }
        public ContextFixture()
        {
            var dbContextOptions = new DbContextOptionsBuilder<LibraryDbContext>().UseInMemoryDatabase(databaseName: "libraryDbInMemory").Options;

            var mockConfiguration = new Mock<IConfiguration>();
            var mockUserResolverService = new Mock<IUserResolverService>();
            mockUserResolverService.Setup(service => service.GetUserName()).Returns("testUser");


            InMemoryDbContext = new LibraryDbContext(dbContextOptions, mockConfiguration.Object, mockUserResolverService.Object);

            var iis = new LibraryDbContext(dbContextOptions, mockConfiguration.Object, mockUserResolverService.Object);
           // iis.Items.

            

            AutoMapper = new MapperConfiguration(options =>
            {
                options.AddProfile<AutoMapperProfile>();
            }).CreateMapper();

            _configuration = new ConfigurationBuilder().AddJsonFile("../appsettings.test.json").Build();

            var sieveOptions = new SieveOptions();
            _configuration.GetSection("Sieve").Bind(sieveOptions);

            SieveProcessor = new ApplicationSieveProcessor(Options.Create(sieveOptions));
        }

        public IMapper AutoMapper { get; private set; }

        public ISieveProcessor SieveProcessor { get; private set; }

        public IUnitOfWork InMemoryDbContext { get; private set; }

        private void SeedData()
        {
            //InMemoryDbContext.Set<Item>().AddRange({
            //    new Item(),
            //    new Item()

            //});

        }

        internal void ClearDatabase()
        {
           // InMemoryDbContext.Set<Item>().
        }
    }
}
