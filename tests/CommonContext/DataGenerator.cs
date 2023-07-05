using Application.Dtos.Identity.Request;
using Application.Dtos.Request;
using AutoMapper;
using Bogus;
using Domain.Entities;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using Profile = Domain.Entities.Profile;

namespace CommonContext
{
    public static class DataGenerator
    {
        private static readonly Dictionary<Type, dynamic> _generators = new Dictionary<Type, dynamic>();
        private static readonly Dictionary<Type, dynamic> _generatorsWithDependencies = new Dictionary<Type, dynamic>();

        private static readonly Dictionary<Type, Type> _domainTypes = new Dictionary<Type, Type>();
        public static readonly IMapper _mapper;

        private static IFormFile? _imageFormFile = null;

        static DataGenerator()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.CreateMap<Item, ItemRequest>();
                config.CreateMap<Library, LibraryRequest>();
                config.CreateMap<IdentityRole, RoleRequest>();
            });

            _mapper = mapperConfiguration.CreateMapper();

            //_domainTypes[typeof(ItemRequest)] = typeof(Item);
            _domainTypes[typeof(RoleRequest)] = typeof(IdentityRole);
            _domainTypes[typeof(LibraryRequest)] = typeof(Library);

            SetGenerators();
        }
        private static void SetGenerators()
        {
            var copyHistoryGenerator = new Faker<CopyHistory>();

            var itemGenerator = new Faker<Item>()
                .RuleFor(item => item.Title, faker => faker.Commerce.ProductName())
                .RuleFor(item => item.Description, faker => faker.Commerce.ProductDescription())
                .RuleFor(item => item.FormOfPublication, faker => faker.PickRandom<Form>())
                .RuleFor(item => item.Authors, faker => faker.Person.FullName)
                .RuleFor(item => item.Publisher, faker => faker.Company.CompanyName())
                .RuleFor(item => item.YearOfPublication, faker => faker.Random.Int(1600, 2022))
                .RuleFor(item => item.ISBN, faker => faker.Commerce.Ean13())
                .RuleFor(item => item.ImagePaths, _ => new List<string>() { "fakeImage.png" });

            var itemRequestGenerator = new Faker<ItemRequest>()
                .RuleFor(itemRequest => itemRequest.Title, faker => faker.Commerce.ProductName())
                .RuleFor(itemRequest => itemRequest.Description, faker => faker.Commerce.ProductDescription())
                .RuleFor(itemRequest => itemRequest.FormOfPublication, faker => faker.PickRandom<Form>())
                .RuleFor(itemRequest => itemRequest.Authors, faker => faker.Person.FullName)
                .RuleFor(itemRequest => itemRequest.Publisher, faker => faker.Company.CompanyName())
                .RuleFor(itemRequest => itemRequest.YearOfPublication, faker => faker.Random.Int(1600, 2022))
                .RuleFor(itemRequest => itemRequest.ISBN, faker => faker.Commerce.Ean13())
                .RuleFor(itemRequest => itemRequest.Images, _ => new List<IFormFile>() { GetImageFormFile() });

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

            var copyGenerator = new Faker<Copy>();

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

            var profileHistoryGenerator = new Faker<ProfileHistory>();

            var profileGenerator = new Faker<Profile>()
               .RuleFor(profile => profile.UserId, faker => "empty_user_id");

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
            _generators[typeof(ItemRequest)] = itemRequestGenerator;
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

            var copyGeneratorWithDependencies = copyGenerator.Clone();
            var rentalGeneratorWithDependencies = rentalGenerator.Clone();
            var archivalRentalGeneratorWithDependencies = archivalRentalGenerator.Clone();
            var reservationGeneratorWithDependencies = reservationGenerator.Clone();
            var archivalReservationGeneratorWithDependencies = archivalReservationGenerator.Clone();
            var profileHistoryGeneratorWithDependencies = profileHistoryGenerator.Clone();
            var copyHistoryGeneratorWithDependencies = copyHistoryGenerator.Clone();
            var profileGeneratorWithDependencies = profileGenerator.Clone();

            copyGeneratorWithDependencies
                .RuleFor(copy => copy.Item, _ => itemGenerator.Generate())
                .RuleFor(copy => copy.Library, _ => libraryGenerator.Generate())
                .RuleFor(copy => copy.CopyHistory, _ => copyHistoryGeneratorWithDependencies.Generate());

            rentalGeneratorWithDependencies
                .RuleFor(rental => rental.Profile, _ => profileGenerator.Generate())
                .RuleFor(rental => rental.Copy, _ => copyGeneratorWithDependencies.Generate());

            reservationGeneratorWithDependencies
                .RuleFor(reservation => reservation.Profile, _ => profileGenerator.Generate())
                .RuleFor(reservation => reservation.Copy, _ => copyGeneratorWithDependencies.Generate());

            archivalRentalGeneratorWithDependencies
                .RuleFor(rental => rental.ProfileHistory, _ => profileHistoryGenerator.Generate())
                .RuleFor(rental => rental.CopyHistory, _ =>
                {
                    var copyHistory = copyHistoryGenerator.Generate();
                    copyHistory.Copy = copyGenerator.Generate();
                    copyHistory.Copy.Item = itemGenerator.Generate();
                    copyHistory.Copy.Library = libraryGenerator.Generate();
                    return copyHistory;
                });

            archivalReservationGeneratorWithDependencies
                .RuleFor(reservation => reservation.ProfileHistory, _ => profileHistoryGenerator.Generate())
                .RuleFor(reservation => reservation.CopyHistory, _ =>
                {
                    var copyHistory = copyHistoryGenerator.Generate();
                    copyHistory.Copy = copyGenerator.Generate();
                    copyHistory.Copy.Item = itemGenerator.Generate();
                    copyHistory.Copy.Library = libraryGenerator.Generate();
                    return copyHistory;
                });


            profileGeneratorWithDependencies
               .RuleFor(profile => profile.ProfileHistory, faker =>
               {
                   var profileHistory = profileHistoryGeneratorWithDependencies.Generate();
                   return profileHistory;
               }
               )
               .RuleFor(profile => profile.CurrentRentals, (_, currentProfile) =>
               {
                   var rental = rentalGeneratorWithDependencies.Generate();
                   rental.Profile = currentProfile;
                   return new List<Rental> { rental };
               }).RuleFor(profile => profile.CurrentReservations, (_, currentProfile) =>
               {
                   var reservation = reservationGeneratorWithDependencies.Generate();
                   reservation.Profile = currentProfile;
                   return new List<Reservation> { reservation };
               });

            profileHistoryGeneratorWithDependencies
               .RuleFor(profileHistory => profileHistory.ArchivalRentals, (_, profileHistory) =>
               {
                   var archival = archivalRentalGeneratorWithDependencies.Generate();
                   archival.ProfileHistory = profileHistory;

                   return new List<ArchivalRental> { archival };
               }).RuleFor(profileHistory => profileHistory.ArchivalReservations, (_, profileHistory) =>
               {
                   var archival = archivalReservationGeneratorWithDependencies.Generate();

                   archival.CopyHistory = profileHistory.ArchivalRentals.First().CopyHistory;
                   archival.ProfileHistory = profileHistory;

                   return new List<ArchivalReservation> { archival };
               });

            copyHistoryGeneratorWithDependencies
            .RuleFor(profileHistory => profileHistory.ArchivalRentals, (_, copyHistory) =>
            {
                var archival = archivalRentalGeneratorWithDependencies.Generate();
                archival.CopyHistory = copyHistory;

                return new List<ArchivalRental> { archival };
            }).RuleFor(profileHistory => profileHistory.ArchivalReservations, (_, copyHistory) =>
            {
                var archival = archivalReservationGeneratorWithDependencies.Generate();

                archival.ProfileHistory = copyHistory.ArchivalRentals.First().ProfileHistory;
                archival.CopyHistory = copyHistory;

                return new List<ArchivalReservation> { archival };
            });

            //itemGenerator.UseSeed(100);
            //registerRequestGenerator.UseSeed(250);
            //applicationUserGenerator.UseSeed(93842421);
            //identityRoleGenerator.UseSeed(750);
            //libraryGenerator.UseSeed(1250);
            //copyGenerator.UseSeed(91476);
            //rentalGenerator.UseSeed(38918);
            //reservationGenerator.UseSeed(2589);
            //profileGenerator.UseSeed(5819);
            //profileHistoryGenerator.UseSeed(19353);
            //archivalRentalGenerator.UseSeed(4715);
            //archivalReservationGenerator.UseSeed(01743);
            //copyHistoryGenerator.UseSeed(2752);

            _generatorsWithDependencies[typeof(Copy)] = copyGeneratorWithDependencies;
            _generatorsWithDependencies[typeof(Rental)] = rentalGeneratorWithDependencies;
            _generatorsWithDependencies[typeof(Reservation)] = reservationGeneratorWithDependencies;
            _generatorsWithDependencies[typeof(ArchivalRental)] = archivalRentalGeneratorWithDependencies;
            _generatorsWithDependencies[typeof(ArchivalReservation)] = archivalReservationGeneratorWithDependencies;
            _generatorsWithDependencies[typeof(ProfileHistory)] = profileHistoryGeneratorWithDependencies;
            _generatorsWithDependencies[typeof(Profile)] = profileGeneratorWithDependencies;
            _generatorsWithDependencies[typeof(CopyHistory)] = copyHistoryGeneratorWithDependencies;
        }
        private static byte[] ReadFileBytes(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static IEnumerable<T> Get<T>(int number)
        {
            return _generators[typeof(T)].Generate(number);
        }
        public static T GetOne<T>()
        {
            return _generators[typeof(T)].Generate();
        }
        public static IEnumerable<T> GetWithDependencies<T>(int number)
        {
            return _generatorsWithDependencies[typeof(T)].Generate(number);
        }
        public static T GetOneWithDependencies<T>()
        {
            return _generatorsWithDependencies[typeof(T)].Generate();
        }
        public static IEnumerable<TRequest> GetRequest<TRequest>(int number)
        {
            var domainEntities = _generators[_domainTypes[typeof(TRequest)]].Generate(number);

            return _mapper.Map<IEnumerable<TRequest>>(domainEntities);
        }
        public static MultipartFormDataContent GetMultipartFormDataContent<T>(T model)
        {
            MultipartFormDataContent formData = new MultipartFormDataContent();

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                object? value = propertyInfo.GetValue(model);

                if (value is null)
                {
                    continue;
                }

                if (value is ICollection<IFormFile> fileCollection)
                {
                    foreach (var file in fileCollection)
                    {
                        var fileContent = new ByteArrayContent(ReadFileBytes(file));
                        formData.Add(fileContent, propertyInfo.Name, file.FileName);//sprawdzic
                    }
                }
                else
                {
                    var stringContent = new StringContent(value.ToString());
                    formData.Add(stringContent, propertyInfo.Name);
                }
            }

            return formData;
        }
        public static MultipartFormDataContent GetMultipartFormDataContentFromCollection<T>(ICollection<T> models, string collectionName)
        {
            MultipartFormDataContent formData = new MultipartFormDataContent();

            var properties = typeof(T).GetProperties();
            var i = 0;

            foreach (var model in models)
            {
                foreach (var propertyInfo in properties)
                {
                    var value = propertyInfo.GetValue(model);

                    if (value is null)
                    {
                        continue;
                    }

                    if (value is ICollection<IFormFile> fileCollection)
                    {
                        foreach (var file in fileCollection)
                        {
                            var fileContent = new ByteArrayContent(ReadFileBytes(file));

                            formData.Add(fileContent, $"{collectionName}[{i}].{propertyInfo.Name}", file.FileName);
                        }
                    }
                    else
                    {
                        var stringContent = new StringContent(value.ToString());

                        formData.Add(stringContent, $"{collectionName}[{i}].{propertyInfo.Name}");
                    }
                }

                i++;
            }

            return formData;
        }
        public static IFormFile GetImageFormFile()
        {
            if (_imageFormFile is not null)
            {
                return _imageFormFile;
            }

            _imageFormFile = GetImageFormFile("image.png");

            return _imageFormFile;
        }
        public static IFormFile GetImageFormFile(string fileName)
        {
            return GetImageFormFile(fileName, Color.Blue);
        }
        public static IFormFile GetImageFormFile(string fileName, Color color)
        {
            Bitmap image = new Bitmap(200, 200);

            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(color);
            }

            var memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Png);
            //memoryStream.Position = 0;

            var imageFormFile = new FormFile(memoryStream, 0, memoryStream.Length, "", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            return imageFormFile;

        }
        public static string GetUserPassword { get; } = "Hn5@68Hhbm*9h2b3h";
        public static string GetOtherUserPassword { get; } = "9jNNbj$@IBYF8Vjdw";
    }
}