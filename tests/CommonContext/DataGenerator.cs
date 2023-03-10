using Application.Dtos.Identity.Request;
using Application.Dtos.Request;
using AutoMapper;
using Bogus;
using Domain.Entities;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace CommonContext
{
    public static class DataGenerator
    {
        private static readonly Dictionary<Type, dynamic> _generators = new Dictionary<Type, dynamic>(); 
        private static readonly Dictionary<Type, Type> _domainTypes = new Dictionary<Type, Type>();
        public static readonly IMapper _mapper;

        static DataGenerator()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.CreateMap<Item, ItemRequest>();
                config.CreateMap<Library, LibraryRequest>();
                config.CreateMap<IdentityRole, RoleRequest>();
            });

            _mapper = mapperConfiguration.CreateMapper();

            _domainTypes[typeof(ItemRequest)] = typeof(Item);
            _domainTypes[typeof(RoleRequest)] = typeof(IdentityRole);
            _domainTypes[typeof(LibraryRequest)] = typeof(Library);


            var itemGenerator = new Faker<Item>()
                .RuleFor(item => item.Title, faker => faker.Commerce.ProductName())
                .RuleFor(item => item.Description, faker => faker.Commerce.ProductDescription())
                .RuleFor(item => item.FormOfPublication, faker => faker.PickRandom<Form>())
                .RuleFor(item => item.Authors, faker => faker.Person.FullName)
                .RuleFor(item => item.Publisher, faker => faker.Company.CompanyName())
                .RuleFor(item => item.YearOfPublication, faker => faker.Random.Int(1600, 2022))
                .RuleFor(item => item.ISBN, faker => faker.Commerce.Ean13());

            var registerRequestGenerator = new Faker<RegisterRequest>()
                .RuleFor(registerRequest => registerRequest.FirstName, faker => faker.Person.FirstName)
                .RuleFor(registerRequest => registerRequest.LastName, faker => faker.Person.LastName)
                .RuleFor(registerRequest => registerRequest.UserName, faker => faker.Person.FullName.Replace(' ', '_'))
                .RuleFor(registerRequest => registerRequest.Password, faker => faker.Internet.Password())
                .RuleFor(registerRequest => registerRequest.ConfirmPassword, (faker, registerRequest) => registerRequest.Password)
                .RuleFor(registerRequest => registerRequest.Email, faker => faker.Internet.Email());

            var applicationUserGenerator = new Faker<ApplicationUser>()
               .RuleFor(user => user.FirstName, faker => faker.Person.FirstName)
               .RuleFor(user => user.LastName, faker => faker.Person.LastName)
               .RuleFor(user => user.UserName, faker => faker.Person.FullName.Replace(' ', '_'))
               .RuleFor(user => user.Email, faker => faker.Internet.Email());

            var identityRoleGenerator = new Faker<IdentityRole>()
               .RuleFor(role => role.Name, faker => "_" + faker.Name.JobType())
               .RuleFor(role => role.NormalizedName, (faker, role) => role.Name.ToUpper());

            var libraryGenerator = new Faker<Library>()
               .RuleFor(Library => Library.Name, faker => faker.Company.CompanyName())
               .RuleFor(Library => Library.Description, faker => faker.Name.JobDescriptor())
               .RuleFor(Library => Library.Address, faker => faker.Address.FullAddress())
               .RuleFor(Library => Library.Email, faker => faker.Internet.Email())
               .RuleFor(Library => Library.PhoneNumber, faker => faker.Person.Phone)
               .RuleFor(Library => Library.NumberOfComputerStations, faker => faker.Random.Number(10))
               .RuleFor(Library => Library.IsScanner, faker => faker.Random.Bool())
               .RuleFor(Library => Library.IsPrinter, faker => faker.Random.Bool())
               .RuleFor(Library => Library.IsPhotocopier, faker => faker.Random.Bool());

            itemGenerator.UseSeed(100);
            registerRequestGenerator.UseSeed(250);
            applicationUserGenerator.UseSeed(93842421);
            identityRoleGenerator.UseSeed(750);
            libraryGenerator.UseSeed(1250);

            _generators[typeof(Item)] = itemGenerator;
            _generators[typeof(RegisterRequest)] = registerRequestGenerator;
            _generators[typeof(ApplicationUser)] = applicationUserGenerator;
            _generators[typeof(IdentityRole)] = identityRoleGenerator;
            _generators[typeof(Library)] = libraryGenerator;
        }

        public static IEnumerable<T> Get<T>(int number)
        {
            return _generators[typeof(T)].Generate(number);
        }

        public static IEnumerable<TRequest> GetRequest<TRequest>(int number)
        {
            var domainEntities = _generators[_domainTypes[typeof(TRequest)]].Generate(number);

            return _mapper.Map<IEnumerable<TRequest>>(domainEntities);
        }

        public static string GetUserPassword { get; } = "Hn5@68Hhbm*9h2b3h";
        public static string GetOtherUserPassword { get; } = "9jNNbj$@IBYF8Vjdw";
    }
}
