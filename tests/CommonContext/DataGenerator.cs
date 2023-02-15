using Application.Dtos.Identity.Request;
using Application.Dtos.Request;
using AutoMapper;
using Bogus;
using Domain.Entities;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

// na generyczne zamiana, wlasny automapper na request

namespace CommonContext
{
    public static class DataGenerator
    {
        private static readonly Dictionary<Type, dynamic> _generators = new Dictionary<Type, dynamic>(); //IFakerTInternal
        private static readonly Dictionary<Type, Type> _domainTypes = new Dictionary<Type, Type>();
        public static readonly IMapper _mapper;

        static DataGenerator()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.CreateMap<Item, ItemRequest>();
            });

            _mapper = mapperConfiguration.CreateMapper();

            _domainTypes[typeof(ItemRequest)] = typeof(Item);
            _domainTypes[typeof(RoleRequest)] = typeof(IdentityRole);


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

            var roleRequetsGenerator = new Faker<RoleRequest>()
                .RuleFor(roleRequets => roleRequets.Name, faker => "_" + faker.Name.JobType());

            itemGenerator.UseSeed(100);
            registerRequestGenerator.UseSeed(250);
            applicationUserGenerator.UseSeed(500);
            identityRoleGenerator.UseSeed(750);
            roleRequetsGenerator.UseSeed(1000);

            _generators[typeof(Item)] = itemGenerator;
            _generators[typeof(RegisterRequest)] = registerRequestGenerator;
            _generators[typeof(ApplicationUser)] = applicationUserGenerator;
            _generators[typeof(IdentityRole)] = identityRoleGenerator;
            _generators[typeof(RoleRequest)] = roleRequetsGenerator;
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
