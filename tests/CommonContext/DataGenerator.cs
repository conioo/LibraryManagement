using Application.Dtos.Identity.Request;
using Application.Dtos.Request;
using AutoMapper;
using Bogus;
using Domain.Entities;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Profile = Domain.Entities.Profile;

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

            var copyHistoryGenerator = new Faker<CopyHistory>();

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
                .RuleFor(registerRequest => registerRequest.ConfirmPassword, (_, registerRequest) => registerRequest.Password)
                .RuleFor(registerRequest => registerRequest.Email, faker => faker.Internet.Email());

            var applicationUserGenerator = new Faker<ApplicationUser>()
               .RuleFor(user => user.FirstName, faker => faker.Person.FirstName)
               .RuleFor(user => user.LastName, faker => faker.Person.LastName)
               .RuleFor(user => user.UserName, faker => faker.Person.FullName.Replace(' ', '_'))
               .RuleFor(user => user.Email, faker => faker.Internet.Email());

            var identityRoleGenerator = new Faker<IdentityRole>()
               .RuleFor(role => role.Name, faker => "_" + faker.Name.JobType())
               .RuleFor(role => role.NormalizedName, (_, role) => role.Name.ToUpper());

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

            var copyGenerator = new Faker<Copy>()
                .RuleFor(copy => copy.Item, _ => itemGenerator.Generate())
                .RuleFor(copy => copy.Library, _ => libraryGenerator.Generate())
                .RuleFor(copy => copy.CopyHistory, _ => copyHistoryGenerator.Generate());

            var rentalGenerator = new Faker<Rental>()
                .RuleFor(rental => rental.BeginDate, faker => DateOnly.FromDateTime(faker.Date.Between(DateTime.Now.AddMonths(-1), DateTime.Now)))
                .RuleFor(rental => rental.EndDate, (_, rental) => rental.BeginDate.AddDays(30));

            var reservationGenerator = new Faker<Reservation>()
              .RuleFor(reservation => reservation.BeginDate, faker => DateOnly.FromDateTime(faker.Date.Between(DateTime.Now.AddMonths(-1), DateTime.Now)))
              .RuleFor(reservation => reservation.EndDate, (_, reservation) => reservation.BeginDate.AddDays(7));

            var archivalRentalGenerator = new Faker<ArchivalRental>()
               .RuleFor(archivalRental => archivalRental.PenaltyCharge, faker => faker.Random.Decimal(0, 20))
               .RuleFor(archivalRental => archivalRental.BeginDate, faker => DateOnly.FromDateTime(faker.Date.Between(DateTime.Now.AddMonths(-10), DateTime.Now.AddMonths(-1))))
               .RuleFor(archivalRental => archivalRental.EndDate, (_, rental) => rental.BeginDate.AddDays(30))
               .RuleFor(archivalRental => archivalRental.ReturnedDate, (faker, rental) => faker.Date.BetweenDateOnly(rental.BeginDate.AddDays(1), rental.BeginDate.AddMonths(1)));

            var archivalReservationGenerator = new Faker<ArchivalReservation>()
              .RuleFor(reservation => reservation.BeginDate, faker => DateOnly.FromDateTime(faker.Date.Between(DateTime.Now.AddMonths(-10), DateTime.Now.AddMonths(-1))))
              .RuleFor(reservation => reservation.EndDate, (_, reservation) => reservation.BeginDate.AddDays(7))
              .RuleFor(reservation => reservation.CollectionDate, (faker, reservation) => faker.Date.BetweenDateOnly(reservation.BeginDate.AddDays(1), reservation.EndDate));

            var profileHistoryGenerator = new Faker<ProfileHistory>()
               .RuleFor(profileHistory => profileHistory.ArchivalRentals, _ =>
               {
                   var archival = archivalRentalGenerator.Generate(1).First();
                   archival.Copy = copyGenerator.Generate(1).First();
                   return new List<ArchivalRental> { archival };
               }).RuleFor(profileHistory => profileHistory.ArchivalReservations, _ =>
               {
                   var archival = archivalReservationGenerator.Generate(1).First();
                   archival.Copy = copyGenerator.Generate(1).First();
                   return new List<ArchivalReservation> { archival };
               });

            var profileGenerator = new Faker<Profile>()
               .RuleFor(profile => profile.UserId, faker => "empty_user_id")
               .RuleFor(profile => profile.ProfileHistory, faker => profileHistoryGenerator.Generate(1).First())
               .RuleFor(profile => profile.CurrrentRentals, _ =>
               {
                   var rental = rentalGenerator.Generate(1).First();
                   rental.Copy = copyGenerator.Generate(1).First();
                   return new List<Rental> { rental };
               }).RuleFor(profile => profile.CurrrentReservations, _ =>
               {
                   var reservation = reservationGenerator.Generate(1).First();
                   reservation.Copy = copyGenerator.Generate(1).First();
                   return new List<Reservation> { reservation };
               });

            copyHistoryGenerator = new Faker<CopyHistory>()
            .RuleFor(profileHistory => profileHistory.ArchivalRentals, _ =>
            {
                var archival = archivalRentalGenerator.Generate(1).First();
                return new List<ArchivalRental> { archival };
            }).RuleFor(profileHistory => profileHistory.ArchivalReservations, _ =>
            {
                var archival = archivalReservationGenerator.Generate(1).First();
                return new List<ArchivalReservation> { archival };
            });

            itemGenerator.UseSeed(100);
            registerRequestGenerator.UseSeed(250);
            applicationUserGenerator.UseSeed(93842421);
            identityRoleGenerator.UseSeed(750);
            libraryGenerator.UseSeed(1250);
            copyGenerator.UseSeed(91476);
            rentalGenerator.UseSeed(38918);
            reservationGenerator.UseSeed(2589);
            profileGenerator.UseSeed(5819);
            profileHistoryGenerator.UseSeed(19353);
            archivalRentalGenerator.UseSeed(4715);
            archivalReservationGenerator.UseSeed(01743);
            copyHistoryGenerator.UseSeed(2752);

            _generators[typeof(Item)] = itemGenerator;
            _generators[typeof(RegisterRequest)] = registerRequestGenerator;
            _generators[typeof(ApplicationUser)] = applicationUserGenerator;
            _generators[typeof(IdentityRole)] = identityRoleGenerator;
            _generators[typeof(Library)] = libraryGenerator;
            _generators[typeof(Copy)] = copyGenerator;
            _generators[typeof(Rental)] = rentalGenerator;
            _generators[typeof(Reservation)] = reservationGenerator;
            _generators[typeof(ArchivalRental)] = archivalRentalGenerator;
            _generators[typeof(ArchivalReservation)] = archivalReservationGenerator;
            _generators[typeof(ProfileHistory)] = profileHistoryGenerator;
            _generators[typeof(Profile)] = profileGenerator;
            _generators[typeof(CopyHistory)] = copyHistoryGenerator;
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