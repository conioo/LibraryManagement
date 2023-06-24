using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Reactive.Interfaces;
using CommonContext;
using CommonContext.Extensions;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;
using WebAPITests.Integration.SharedContextBuilders;
using Profile = Domain.Entities.Profile;

namespace WebAPITests.Integration
{
    public class RentalsControllerTests : IClassFixture<RentalContextBuilder>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly Copy _copy;
        private readonly Profile _defaultProfile;
        private readonly ApplicationUser _defaultUser;
        private readonly Profile _rentalProfile;
        private readonly IEnumerable<Rental> _rentals;
        private readonly SharedContext _sharedContext;

        public RentalsControllerTests(RentalContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _defaultUser = _sharedContext.DefaultUser;
            _defaultProfile = _sharedContext.DefaultProfile;

            _copy = DataGenerator.Get<Copy>(1).First();
            _copy.CopyHistory.Clear();

            var copy2 = DataGenerator.Get<Copy>(1).First();
            var copy3 = DataGenerator.Get<Copy>(1).First();

            copy2.IsAvailable = false;
            copy3.IsAvailable = false;
            copy2.CopyHistory.Clear();
            copy3.CopyHistory.Clear();

            _rentalProfile = DataGenerator.Get<Profile>(1).First();
            _rentalProfile.CurrentRentals = null;
            _rentalProfile.CurrentReservations = null;
            _rentalProfile.ProfileHistory.Clear();

            _rentals = DataGenerator.Get<Rental>(2);

            _rentals.ElementAt(0).Copy = copy2;
            _rentals.ElementAt(0).Profile = _rentalProfile;

            _rentals.ElementAt(1).Copy = copy3;
            _rentals.ElementAt(1).Profile = _rentalProfile;

            _sharedContext.DbContext.Set<Copy>().Add(_copy);
            _sharedContext.DbContext.Set<Rental>().AddRange(_rentals);
            _sharedContext.DbContext.SaveChangesAsync().Wait();

            _sharedContext.RefreshDb();
        }

        [Fact]
        async Task AddRentalAsync_ForDeactiveProfile_Returns400BadRequest()
        {
            _defaultProfile.IsActive = false;
            _sharedContext.DbContext.Set<Profile>().Update(_defaultProfile);
            await _sharedContext.DbContext.SaveChangesAsync();

            var rentalRequest = new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRental + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("profile is deactivate");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                   .Include(rProfile => rProfile.CurrentRentals)
                   .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                   .FirstOrDefaultAsync();

            refreshProfile.CurrentRentals.Should().BeNullOrEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalAsync_ForInvalidCopyInventoryNumber_Returns404NotFound()
        {
            var rentalRequest = new RentalRequest() { CopyInventoryNumber = "null_null" };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRental + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("copy not found");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentRentals.Should().BeNullOrEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalAsync_ForInvalidModel_Returns400BadRequest()
        {
            var rentalRequest = new RentalRequest() { CopyInventoryNumber = String.Empty };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRental + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            validationProblemDetails.Errors.Count.Should().Be(1);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentRentals.Should().BeNullOrEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalAsync_ForInvalidProfileLibraryCardNumber_Returns404NotFound()
        {
            var rentalRequest = new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRental + $"?profileLibraryCardNumber=null_null"),
                Content = JsonContent.Create(rentalRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("profile not found");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalAsync_ForMaxRentals_Returns400BadRequest()
        {
            _defaultProfile.CurrentRentals = (ICollection<Rental>)DataGenerator.Get<Rental>(5);

            _sharedContext.DbContext.Set<Profile>().Update(_defaultProfile);
            await _sharedContext.DbContext.SaveChangesAsync();

            var rentalRequest = new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRental + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("user has a maximum number of rentals");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(7);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                   .Include(rProfile => rProfile.CurrentRentals)
                   .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                   .FirstOrDefaultAsync();

            refreshProfile.CurrentRentals.Count().Should().Be(5);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalAsync_ForUnavailableCopy_Returns400BadRequest()
        {
            _copy.IsAvailable = false;

            _sharedContext.DbContext.Set<Copy>().Update(_copy);
            await _sharedContext.DbContext.SaveChangesAsync();

            var rentalRequest = new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRental + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("copy doesn't available");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentRentals.Should().BeNullOrEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalAsync_ForValidModel_Returns201Created()
        {
            var rentalRequest = new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRental + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            var responseRental = await response.Content.ReadFromJsonAsync<RentalResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Rentals.Prefix}/{Rentals.GetRentalById}?id={responseRental.Id}");

            response.Headers.Location.Should().Be(expectedLocationUri);


            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(3);

            var newRental = await _sharedContext.DbContext.Set<Rental>()
                .Include(rental => rental.Copy)
                .Include(rental => rental.Profile)
                .Where(rental => rental.Id == responseRental.Id)
                .FirstOrDefaultAsync();

            newRental.Should().NotBeNull();

            newRental.CreatedBy.Should().Be("default");
            newRental.LastModifiedBy.Should().Be("default");

            newRental.Created.Should().NotBe(null).And.Be(newRental.LastModified);

            newRental.Copy.InventoryNumber.Should().Be(_copy.InventoryNumber);
            newRental.Copy.IsAvailable.Should().BeFalse();
            newRental.Profile.LibraryCardNumber.Should().Be(_defaultProfile.LibraryCardNumber);

            responseRental.Should().BeEquivalentTo(newRental, options => options.ExcludingMissingMembers());

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentRentals.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentRentals.First().Id.Should().Be(newRental.Id);


            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.Is<Rental>(rental => rental.Copy.InventoryNumber == _copy.InventoryNumber && rental.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
        }

        [Fact]
        async Task AddRentalsAsync_ForDeactiveProfile_Returns400BadRequest()
        {
            _defaultProfile.IsActive = false;
            _sharedContext.DbContext.Set<Profile>().Update(_defaultProfile);
            await _sharedContext.DbContext.SaveChangesAsync();

            var rentalsRequest = new List<RentalRequest>() {
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("profile is deactivate");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                   .Include(rProfile => rProfile.CurrentRentals)
                   .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                   .FirstOrDefaultAsync();

            refreshProfile.CurrentRentals.Should().BeNullOrEmpty();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();


            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalsAsync_ForEmptyRentals_Returns400BadRequest()
        {
            var rentalsRequest = new List<RentalRequest>() { };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            validationProblemDetails.Errors.Count().Should().Be(1);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentRentals.Should().BeEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalsAsync_ForInvalidProfileLibraryCardNumber_Returns404NotFound()
        {
            var rentalsRequest = new List<RentalRequest>() {
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber=null_null"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("profile not found");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalsAsync_ForMaxAmountRentals_Returns200Ok()
        {
            var copies = DataGenerator.Get<Copy>(4).ToList();
            _sharedContext.DbContext.Set<Copy>().AddRange(copies);
            await _sharedContext.DbContext.SaveChangesAsync();

            var rentalsRequest = new List<RentalRequest>() {
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new RentalRequest() { CopyInventoryNumber = copies[0].InventoryNumber  },
                new RentalRequest() { CopyInventoryNumber = copies[1].InventoryNumber  },
                new RentalRequest() { CopyInventoryNumber = copies[2].InventoryNumber  },
                new RentalRequest() { CopyInventoryNumber = copies[3].InventoryNumber  } };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(7);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);
            var copy2 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[0].InventoryNumber);
            var copy3 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[1].InventoryNumber);
            var copy4 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[2].InventoryNumber);
            var copy5 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[3].InventoryNumber);

            copy1.IsAvailable.Should().BeFalse();
            copy2.IsAvailable.Should().BeFalse();
            copy3.IsAvailable.Should().BeFalse();
            copy4.IsAvailable.Should().BeFalse();
            copy5.IsAvailable.Should().BeFalse();

            refreshProfile.CurrentRentals.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentRentals.Count.Should().Be(5);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.Is<Rental>(rental => rental.Copy.InventoryNumber == _copy.InventoryNumber && rental.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.Is<Rental>(rental => rental.Copy.InventoryNumber == copies[0].InventoryNumber && rental.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.Is<Rental>(rental => rental.Copy.InventoryNumber == copies[1].InventoryNumber && rental.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.Is<Rental>(rental => rental.Copy.InventoryNumber == copies[2].InventoryNumber && rental.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.Is<Rental>(rental => rental.Copy.InventoryNumber == copies[3].InventoryNumber && rental.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
        }

        [Fact]
        async Task AddRentalsAsync_ForOneCopyTwoTimes_Returns400BadRequest()
        {
            var rentalsRequest = new List<RentalRequest>() {
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber  }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            validationProblemDetails.Errors.Count().Should().Be(1);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();

            refreshProfile.CurrentRentals.Should().BeEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalsAsync_ForOneInvalidCopyInventoryNumber_Returns404NotFound()
        {
            var rentalsRequest = new List<RentalRequest>() {
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new RentalRequest() { CopyInventoryNumber = "null_null"  }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var stringResponse = await response.Content.ReadAsStringAsync();

            stringResponse.Should().Be($"copy null_null not found");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();

            refreshProfile.CurrentRentals.Should().BeEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalsAsync_ForOneInvalidModel_Returns400BadRequest()
        {
            var rentalsRequest = new List<RentalRequest>() {
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new RentalRequest() { CopyInventoryNumber = ""  }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            validationProblemDetails.Errors.Count().Should().Be(1);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();

            refreshProfile.CurrentRentals.Should().BeEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalsAsync_ForOneUnavailableCopy_Returns400BadRequest()
        {
            var copies = DataGenerator.Get<Copy>(1).ToList();
            copies[0].IsAvailable = false;
            _sharedContext.DbContext.Set<Copy>().AddRange(copies);
            await _sharedContext.DbContext.SaveChangesAsync();

            var rentalsRequest = new List<RentalRequest>() {
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new RentalRequest() { CopyInventoryNumber = copies[0].InventoryNumber  }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be($"copy {copies[0].InventoryNumber} doesn't available");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);
            var copy2 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[0].InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();
            copy2.IsAvailable.Should().BeFalse();

            refreshProfile.CurrentRentals.Should().BeEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalsAsync_ForTooManyRentals_Returns400BadRequest()
        {
            var copies = DataGenerator.Get<Copy>(5).ToList();
            _sharedContext.DbContext.Set<Copy>().AddRange(copies);
            await _sharedContext.DbContext.SaveChangesAsync();

            var rentalsRequest = new List<RentalRequest>() {
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new RentalRequest() { CopyInventoryNumber = copies[0].InventoryNumber  },
                new RentalRequest() { CopyInventoryNumber = copies[1].InventoryNumber  },
                new RentalRequest() { CopyInventoryNumber = copies[2].InventoryNumber  },
                new RentalRequest() { CopyInventoryNumber = copies[3].InventoryNumber  },
                new RentalRequest() { CopyInventoryNumber = copies[4].InventoryNumber  },};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Be("it isn't possible to add so many rentals");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);
            var copy2 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[0].InventoryNumber);
            var copy3 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[1].InventoryNumber);
            var copy4 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[2].InventoryNumber);
            var copy5 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[3].InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();
            copy2.IsAvailable.Should().BeTrue();
            copy3.IsAvailable.Should().BeTrue();
            copy4.IsAvailable.Should().BeTrue();
            copy5.IsAvailable.Should().BeTrue();

            refreshProfile.CurrentRentals.Should().BeEmpty();

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task AddRentalsAsync_ForValidModels_Returns200Ok()
        {
            var copy = DataGenerator.Get<Copy>(1).First();
            _sharedContext.DbContext.Set<Copy>().Add(copy);
            await _sharedContext.DbContext.SaveChangesAsync();

            var rentalsRequest = new List<RentalRequest>() {
                new RentalRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new RentalRequest() { CopyInventoryNumber = copy.InventoryNumber  } };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Rentals.AddRentals + $"?profileLibraryCardNumber={_defaultProfile.LibraryCardNumber}"),
                Content = JsonContent.Create(rentalsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(4);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);
            var copy2 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copy.InventoryNumber);

            copy1.IsAvailable.Should().BeFalse();
            copy2.IsAvailable.Should().BeFalse();

            refreshedProfile.CurrentRentals.Should().NotBeNullOrEmpty();
            refreshedProfile.CurrentRentals.Count.Should().Be(2);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.Is<Rental>(rental => rental.Copy.InventoryNumber == _copy.InventoryNumber && rental.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
            countingOfPenaltyChangesMock.Verify(service => service.AddRental(It.Is<Rental>(rental => rental.Copy.InventoryNumber == copy.InventoryNumber && rental.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
        }

        [Fact]
        async Task GetRentalByIdAsync_ForInvalidId_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Rentals.GetRentalById, "id", "null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetRentalByIdAsync_ForValidId_Returns200Ok()
        {
            var requestUri = QueryHelpers.AddQueryString(Rentals.GetRentalById, "id", _rentals.First().Id);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<RentalResponse>();

            _sharedContext.RefreshDb();

            result.Should().BeEquivalentTo(_rentals.First(), options => options.ExcludingMissingMembers());
            result.ItemTitle.Should().NotBeNull().And.Be(_rentals.First().Copy.Item.Title);
        }

        [Fact]
        async Task PayThePenaltyAsync_ForTheSecondCall_Returns404NotFound()
        {
            await _sharedContext.Cache.SetStringAsync(_rentals.First().Id, String.Empty, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.PayThePenalty + $"?id={_rentals.First().Id}"),
            };

            var request2 = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.PayThePenalty + $"?id={_rentals.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            response = await _client.SendAsync(request2);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(1);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(1);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.Is<Rental>(rental => rental.Id == _rentals.First().Id)), Times.Once);
        }

        [Fact]
        async Task PayThePenaltyAsync_ForValidIdAfterReturn_Returns200Ok()
        {
            await _sharedContext.Cache.SetStringAsync(_rentals.First().Id, String.Empty, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.PayThePenalty + $"?id={_rentals.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(1);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(1);

            var refreshedCopy = await _sharedContext.DbContext.Set<Copy>()
                .Include(copy => copy.CopyHistory)
                .ThenInclude(CopyHistory => CopyHistory.ArchivalRentals)
                .Include(copy => copy.CurrentRental)
                .Where(copy => copy.InventoryNumber == _rentals.First().Copy.InventoryNumber)
                .FirstOrDefaultAsync();

            refreshedCopy.Should().NotBeNull();
            refreshedCopy.CurrentRental.Should().BeNull();
            refreshedCopy.IsAvailable.Should().BeTrue();
            refreshedCopy.CopyHistory.ArchivalRentals.Count().Should().Be(1);
            refreshedCopy.CopyHistory.ArchivalRentals.First().Id.Should().Be(_rentals.First().Id);
            refreshedCopy.LastModified.Should().BeAfter(refreshedCopy.Created);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                     .Include(profile => profile.ProfileHistory)
                     .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                     .Include(profile => profile.CurrentRentals)
                     .Where(rProfile => rProfile.LibraryCardNumber == _rentalProfile.LibraryCardNumber)
                     .FirstOrDefaultAsync();

            refreshedProfile.CurrentRentals.Count().Should().Be(1);
            refreshedProfile.ProfileHistory.ArchivalRentals.Count().Should().Be(1);
            refreshedProfile.ProfileHistory.ArchivalRentals.First().Id.Should().Be(_rentals.First().Id);


            var archivalRental = await _sharedContext.DbContext.Set<ArchivalRental>().FindAsync(_rentals.First().Id);

            archivalRental.BeginDate.Should().Be(_rentals.First().BeginDate);
            archivalRental.EndDate.Should().Be(_rentals.First().EndDate);
            archivalRental.PenaltyCharge.Should().Be(_rentals.First().PenaltyCharge);
            archivalRental.NumberOfRenewals.Should().Be(_rentals.First().NumberOfRenewals);
            archivalRental.ProfileHistory.Profile.LibraryCardNumber.Should().Be(_rentals.First().Profile.LibraryCardNumber);
            archivalRental.CopyHistory.Copy.InventoryNumber.Should().Be(_rentals.First().Copy.InventoryNumber);
            archivalRental.ReturnedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.Is<Rental>(rental => rental.Id == _rentals.First().Id)), Times.Once);
        }

        [Fact]
        async Task PayThePenaltyAsync_ForValidIdWithoutReturn_Returns400BadRequest()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.PayThePenalty + $"?id={_rentals.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(0);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task RemoveRentalByIdAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Rentals.RemoveRentalById + $"?id=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);
        }

        [Fact]
        async Task RemoveRentalByIdAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Rentals.RemoveRentalById + $"?id={_rentals.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(1);
        }

        [Fact]
        async Task RenewalAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Renewal + $"?id=null_null"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.RenewalRental(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Never);
        }

        [Fact]
        async Task RenewalAsync_ForRenewalLimitExceeded_Returns400BadRequest()
        {
            _rentals.First().NumberOfRenewals = 2;
            _rentals.First().EndDate = _rentals.First().EndDate.AddDays(60);

            _sharedContext.DbContext.Set<Rental>().Update(_rentals.First());
            await _sharedContext.DbContext.SaveChangesAsync();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Renewal + $"?id={_rentals.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("Maximum number of renewal has already been used");

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.RenewalRental(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Never);
        }

        [Fact]
        async Task RenewalAsync_ForTheLastRenewal_Returns200Ok()
        {
            _rentals.First().NumberOfRenewals = 1;
            _rentals.First().EndDate = _rentals.First().EndDate.AddDays(30);

            _sharedContext.DbContext.Set<Rental>().Update(_rentals.First());
            await _sharedContext.DbContext.SaveChangesAsync();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Renewal + $"?id={_rentals.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var renewalRental = await _sharedContext.DbContext.Set<Rental>().FindAsync(_rentals.First().Id);

            renewalRental.Should().NotBeNull();

            renewalRental.LastModified.Should().BeAfter(renewalRental.Created);
            renewalRental.NumberOfRenewals.Should().Be(2);
            renewalRental.EndDate.Should().Be(renewalRental.BeginDate.AddDays(90));

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.RenewalRental(It.Is<string>(id => id == _rentals.First().Id), It.Is<DateOnly>(dateOnly => dateOnly == renewalRental.BeginDate.AddDays(60)), It.Is<DateOnly>(dateOnly => dateOnly == renewalRental.BeginDate.AddDays(90))), Times.Once);
        }

        [Fact]
        async Task RenewalAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Renewal + $"?id={_rentals.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var renewalRental = await _sharedContext.DbContext.Set<Rental>().FindAsync(_rentals.First().Id);

            renewalRental.Should().NotBeNull();

            renewalRental.LastModified.Should().BeAfter(renewalRental.Created);
            renewalRental.NumberOfRenewals.Should().Be(1);
            renewalRental.EndDate.Should().Be(renewalRental.BeginDate.AddDays(60));

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChangesMock.Verify(service => service.RenewalRental(It.Is<string>(id => id == _rentals.First().Id), It.Is<DateOnly>(dateOnly => dateOnly == renewalRental.BeginDate.AddDays(30)), It.Is<DateOnly>(dateOnly => dateOnly == renewalRental.BeginDate.AddDays(60))), Times.Once);
        }

        [Fact]
        async Task ReturnAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Return + $"?id=null_null"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(0);

            var refreshedCopy = await _sharedContext.DbContext.Set<Copy>()
                .Include(copy => copy.CopyHistory)
                .ThenInclude(CopyHistory => CopyHistory.ArchivalRentals)
                .Include(copy => copy.CurrentRental)
                .Where(copy => copy.InventoryNumber == _rentals.First().Copy.InventoryNumber)
                .FirstOrDefaultAsync();

            refreshedCopy.Should().NotBeNull();
            refreshedCopy.CurrentRental.Should().NotBeNull();
            refreshedCopy.IsAvailable.Should().BeFalse();
            refreshedCopy.CopyHistory.ArchivalRentals.Count().Should().Be(0);
            refreshedCopy.LastModified.Should().Be(refreshedCopy.Created);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                     .Include(profile => profile.ProfileHistory)
                     .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                     .Include(profile => profile.CurrentRentals)
                     .Where(rProfile => rProfile.LibraryCardNumber == _rentalProfile.LibraryCardNumber)
                     .FirstOrDefaultAsync();

            refreshedProfile.CurrentRentals.Count().Should().Be(2);
            refreshedProfile.ProfileHistory.ArchivalRentals.Count().Should().Be(0);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task ReturnAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Return + $"?id={_rentals.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(1);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(1);

            var refreshedCopy = await _sharedContext.DbContext.Set<Copy>()
                .Include(copy => copy.CopyHistory)
                .ThenInclude(CopyHistory => CopyHistory.ArchivalRentals)
                .Include(copy => copy.CurrentRental)
                .Where(copy => copy.InventoryNumber == _rentals.First().Copy.InventoryNumber)
                .FirstOrDefaultAsync();

            refreshedCopy.Should().NotBeNull();
            refreshedCopy.CurrentRental.Should().BeNull();
            refreshedCopy.IsAvailable.Should().BeTrue();
            refreshedCopy.CopyHistory.ArchivalRentals.Count().Should().Be(1);
            refreshedCopy.CopyHistory.ArchivalRentals.First().Id.Should().Be(_rentals.First().Id);
            refreshedCopy.LastModified.Should().BeAfter(refreshedCopy.Created);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                     .Include(profile => profile.ProfileHistory)
                     .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                     .Include(profile => profile.CurrentRentals)
                     .Where(rProfile => rProfile.LibraryCardNumber == _rentalProfile.LibraryCardNumber)
                     .FirstOrDefaultAsync();

            refreshedProfile.CurrentRentals.Count().Should().Be(1);
            refreshedProfile.ProfileHistory.ArchivalRentals.Count().Should().Be(1);
            refreshedProfile.ProfileHistory.ArchivalRentals.First().Id.Should().Be(_rentals.First().Id);


            var archivalRental = await _sharedContext.DbContext.Set<ArchivalRental>().FindAsync(_rentals.First().Id);

            archivalRental.BeginDate.Should().Be(_rentals.First().BeginDate);
            archivalRental.EndDate.Should().Be(_rentals.First().EndDate);
            archivalRental.PenaltyCharge.Should().Be(_rentals.First().PenaltyCharge);
            archivalRental.NumberOfRenewals.Should().Be(_rentals.First().NumberOfRenewals);
            archivalRental.ProfileHistory.Profile.LibraryCardNumber.Should().Be(_rentals.First().Profile.LibraryCardNumber);
            archivalRental.CopyHistory.Copy.InventoryNumber.Should().Be(_rentals.First().Copy.InventoryNumber);
            archivalRental.ReturnedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.Is<Rental>(rental => rental.Id == _rentals.First().Id)), Times.Once);
        }

        [Fact]
        async Task ReturnAsync_LateReturn_Returns200Ok()
        {
            _rentals.First().PenaltyCharge = 15;

            _sharedContext.DbContext.Set<Rental>().Update(_rentals.First());
            _sharedContext.DbContext.SaveChanges();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Return + $"?id={_rentals.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            response.Headers.Contains("payment-required").Should().BeTrue();
            response.Headers.GetValues("payment-required").First().Should().Be($"{_rentals.First().Id}");

            _sharedContext.Cache.GetString(_rentals.First().Id).Should().NotBeNull();

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(0);

            var refreshedCopy = await _sharedContext.DbContext.Set<Copy>()
                .Include(copy => copy.CopyHistory)
                .ThenInclude(CopyHistory => CopyHistory.ArchivalRentals)
                .Include(copy => copy.CurrentRental)
                .Where(copy => copy.InventoryNumber == _rentals.First().Copy.InventoryNumber)
                .FirstOrDefaultAsync();

            refreshedCopy.Should().NotBeNull();
            refreshedCopy.CurrentRental.Should().NotBeNull();
            refreshedCopy.IsAvailable.Should().BeFalse();
            refreshedCopy.CopyHistory.ArchivalRentals.Count().Should().Be(0);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                     .Include(profile => profile.ProfileHistory)
                     .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                     .Include(profile => profile.CurrentRentals)
                     .Where(rProfile => rProfile.LibraryCardNumber == _rentalProfile.LibraryCardNumber)
                     .FirstOrDefaultAsync();

            refreshedProfile.CurrentRentals.Count().Should().Be(2);
            refreshedProfile.ProfileHistory.ArchivalRentals.Count().Should().Be(0);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task ReturnsAsync_ForOneEmptyID_Returns400BadRequest()
        {
            var ids = new List<string>()
            {
                _rentals.ElementAt(0).Id,
                ""
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Returns),
                Content = JsonContent.Create(ids)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(1);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task ReturnsAsync_ForOneInvalidId_Returns404NotFound()
        {
            var ids = new List<string>()
            {
                _rentals.ElementAt(0).Id,
                "null_null"
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Returns),
                Content = JsonContent.Create(ids)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("rental with id: null_null not found");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(0);

            var refreshedCopy0 = await _sharedContext.DbContext.Set<Copy>()
                .Include(copy => copy.CopyHistory)
                .ThenInclude(CopyHistory => CopyHistory.ArchivalRentals)
                .Include(copy => copy.CurrentRental)
                .Where(copy => copy.InventoryNumber == _rentals.ElementAt(0).Copy.InventoryNumber)
                .FirstOrDefaultAsync();

            refreshedCopy0.Should().NotBeNull();
            refreshedCopy0.CurrentRental.Should().NotBeNull();
            refreshedCopy0.IsAvailable.Should().BeFalse();
            refreshedCopy0.CopyHistory.ArchivalRentals.Count().Should().Be(0);
            refreshedCopy0.LastModified.Should().Be(refreshedCopy0.Created);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                     .Include(profile => profile.ProfileHistory)
                     .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                     .Include(profile => profile.CurrentRentals)
                     .Where(rProfile => rProfile.LibraryCardNumber == _rentalProfile.LibraryCardNumber)
                     .FirstOrDefaultAsync();

            refreshedProfile.CurrentRentals.Count().Should().Be(2);
            refreshedProfile.ProfileHistory.ArchivalRentals.Count().Should().Be(0);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task ReturnsAsync_ForOneLateReturn_Returns200Ok()
        {
            _rentals.First().PenaltyCharge = 15;

            _sharedContext.DbContext.Set<Rental>().Update(_rentals.First());
            _sharedContext.DbContext.SaveChanges();

            var ids = new List<string>()
            {
                _rentals.ElementAt(0).Id,
                _rentals.ElementAt(1).Id
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Returns),
                Content = JsonContent.Create(ids)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            response.Headers.Contains("payment-required").Should().BeTrue();
            response.Headers.GetValues("payment-required").First().Should().Be($"{_rentals.First().Id}");

            _sharedContext.Cache.GetString(_rentals.First().Id).Should().NotBeNull();


            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(1);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(1);

            var refreshedCopy0 = await _sharedContext.DbContext.Set<Copy>()
                .Include(copy => copy.CopyHistory)
                .ThenInclude(CopyHistory => CopyHistory.ArchivalRentals)
                .Include(copy => copy.CurrentRental)
                .Where(copy => copy.InventoryNumber == _rentals.ElementAt(0).Copy.InventoryNumber)
                .FirstOrDefaultAsync();

            refreshedCopy0.Should().NotBeNull();
            refreshedCopy0.CurrentRental.Should().NotBeNull();
            refreshedCopy0.IsAvailable.Should().BeFalse();
            refreshedCopy0.CopyHistory.ArchivalRentals.Count().Should().Be(0);

            var refreshedCopy1 = await _sharedContext.DbContext.Set<Copy>()
               .Include(copy => copy.CopyHistory)
               .ThenInclude(CopyHistory => CopyHistory.ArchivalRentals)
               .Include(copy => copy.CurrentRental)
               .Where(copy => copy.InventoryNumber == _rentals.ElementAt(1).Copy.InventoryNumber)
               .FirstOrDefaultAsync();

            refreshedCopy1.Should().NotBeNull();
            refreshedCopy1.CurrentRental.Should().BeNull();
            refreshedCopy1.IsAvailable.Should().BeTrue();
            refreshedCopy1.CopyHistory.ArchivalRentals.Count().Should().Be(1);
            refreshedCopy1.CopyHistory.ArchivalRentals.First().Id.Should().Be(_rentals.ElementAt(1).Id);
            refreshedCopy1.LastModified.Should().BeAfter(refreshedCopy1.Created);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                     .Include(profile => profile.ProfileHistory)
                     .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                     .Include(profile => profile.CurrentRentals)
                     .Where(rProfile => rProfile.LibraryCardNumber == _rentalProfile.LibraryCardNumber)
                     .FirstOrDefaultAsync();

            refreshedProfile.CurrentRentals.Count().Should().Be(1);
            refreshedProfile.ProfileHistory.ArchivalRentals.Count().Should().Be(1);
            refreshedProfile.ProfileHistory.ArchivalRentals.ElementAt(0).Id.Should().Be(_rentals.ElementAt(1).Id);

            var archivalRental1 = await _sharedContext.DbContext.Set<ArchivalRental>().FindAsync(_rentals.ElementAt(1).Id);

            archivalRental1.BeginDate.Should().Be(_rentals.ElementAt(1).BeginDate);
            archivalRental1.EndDate.Should().Be(_rentals.ElementAt(1).EndDate);
            archivalRental1.PenaltyCharge.Should().Be(_rentals.ElementAt(1).PenaltyCharge);
            archivalRental1.NumberOfRenewals.Should().Be(_rentals.ElementAt(1).NumberOfRenewals);
            archivalRental1.ProfileHistory.Profile.LibraryCardNumber.Should().Be(_rentals.ElementAt(1).Profile.LibraryCardNumber);
            archivalRental1.CopyHistory.Copy.InventoryNumber.Should().Be(_rentals.ElementAt(1).Copy.InventoryNumber);
            archivalRental1.ReturnedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.Is<Rental>(rental => rental.Id == _rentals.ElementAt(1).Id)), Times.Once);
        }

        [Fact]
        async Task ReturnsAsync_ForTwoOfTheSameId_Returns400BadRequest()
        {
            var ids = new List<string>()
            {
                _rentals.ElementAt(0).Id,
                _rentals.ElementAt(0).Id
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Returns),
                Content = JsonContent.Create(ids)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("Id repetition detected");

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(2);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(0);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                     .Include(profile => profile.ProfileHistory)
                     .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                     .Include(profile => profile.CurrentRentals)
                     .Where(rProfile => rProfile.LibraryCardNumber == _rentalProfile.LibraryCardNumber)
                     .FirstOrDefaultAsync();

            refreshedProfile.CurrentRentals.Count().Should().Be(2);
            refreshedProfile.ProfileHistory.ArchivalRentals.Count().Should().Be(0);

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        async Task ReturnsAsync_ForValidIds_Returns200Ok()
        {
            var ids = new List<string>()
            {
                _rentals.ElementAt(0).Id,
                _rentals.ElementAt(1).Id
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Rentals.Returns),
                Content = JsonContent.Create(ids)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(0);
            _sharedContext.DbContext.Set<ArchivalRental>().Count().Should().Be(2);

            var refreshedCopy0 = await _sharedContext.DbContext.Set<Copy>()
                .Include(copy => copy.CopyHistory)
                .ThenInclude(CopyHistory => CopyHistory.ArchivalRentals)
                .Include(copy => copy.CurrentRental)
                .Where(copy => copy.InventoryNumber == _rentals.ElementAt(0).Copy.InventoryNumber)
                .FirstOrDefaultAsync();

            refreshedCopy0.Should().NotBeNull();
            refreshedCopy0.CurrentRental.Should().BeNull();
            refreshedCopy0.IsAvailable.Should().BeTrue();
            refreshedCopy0.CopyHistory.ArchivalRentals.Count().Should().Be(1);
            refreshedCopy0.CopyHistory.ArchivalRentals.First().Id.Should().Be(_rentals.ElementAt(0).Id);
            refreshedCopy0.LastModified.Should().BeAfter(refreshedCopy0.Created);

            var refreshedCopy1 = await _sharedContext.DbContext.Set<Copy>()
               .Include(copy => copy.CopyHistory)
               .ThenInclude(CopyHistory => CopyHistory.ArchivalRentals)
               .Include(copy => copy.CurrentRental)
               .Where(copy => copy.InventoryNumber == _rentals.ElementAt(1).Copy.InventoryNumber)
               .FirstOrDefaultAsync();

            refreshedCopy1.Should().NotBeNull();
            refreshedCopy1.CurrentRental.Should().BeNull();
            refreshedCopy1.IsAvailable.Should().BeTrue();
            refreshedCopy1.CopyHistory.ArchivalRentals.Count().Should().Be(1);
            refreshedCopy1.CopyHistory.ArchivalRentals.First().Id.Should().Be(_rentals.ElementAt(1).Id);
            refreshedCopy1.LastModified.Should().BeAfter(refreshedCopy1.Created);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                     .Include(profile => profile.ProfileHistory)
                     .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                     .Include(profile => profile.CurrentRentals)
                     .Where(rProfile => rProfile.LibraryCardNumber == _rentalProfile.LibraryCardNumber)
                     .FirstOrDefaultAsync();

            refreshedProfile.CurrentRentals.Count().Should().Be(0);
            refreshedProfile.ProfileHistory.ArchivalRentals.Count().Should().Be(2);
            refreshedProfile.ProfileHistory.ArchivalRentals.ElementAt(0).Id.Should().Be(_rentals.ElementAt(0).Id);
            refreshedProfile.ProfileHistory.ArchivalRentals.ElementAt(1).Id.Should().Be(_rentals.ElementAt(1).Id);


            var archivalRental0 = await _sharedContext.DbContext.Set<ArchivalRental>().FindAsync(_rentals.ElementAt(0).Id);

            archivalRental0.BeginDate.Should().Be(_rentals.ElementAt(0).BeginDate);
            archivalRental0.EndDate.Should().Be(_rentals.ElementAt(0).EndDate);
            archivalRental0.PenaltyCharge.Should().Be(_rentals.ElementAt(0).PenaltyCharge);
            archivalRental0.NumberOfRenewals.Should().Be(_rentals.ElementAt(0).NumberOfRenewals);
            archivalRental0.ProfileHistory.Profile.LibraryCardNumber.Should().Be(_rentals.ElementAt(0).Profile.LibraryCardNumber);
            archivalRental0.CopyHistory.Copy.InventoryNumber.Should().Be(_rentals.ElementAt(0).Copy.InventoryNumber);
            archivalRental0.ReturnedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));

            var archivalRental1 = await _sharedContext.DbContext.Set<ArchivalRental>().FindAsync(_rentals.ElementAt(1).Id);

            archivalRental1.BeginDate.Should().Be(_rentals.ElementAt(1).BeginDate);
            archivalRental1.EndDate.Should().Be(_rentals.ElementAt(1).EndDate);
            archivalRental1.PenaltyCharge.Should().Be(_rentals.ElementAt(1).PenaltyCharge);
            archivalRental1.NumberOfRenewals.Should().Be(_rentals.ElementAt(1).NumberOfRenewals);
            archivalRental0.ProfileHistory.Profile.LibraryCardNumber.Should().Be(_rentals.ElementAt(1).Profile.LibraryCardNumber);
            archivalRental1.CopyHistory.Copy.InventoryNumber.Should().Be(_rentals.ElementAt(1).Copy.InventoryNumber);
            archivalRental1.ReturnedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));

            var countingOfPenaltyChangesMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.Is<Rental>(rental => rental.Id == _rentals.ElementAt(0).Id)), Times.Once);
            countingOfPenaltyChangesMock.Verify(service => service.ReturnOfItem(It.Is<Rental>(rental => rental.Id == _rentals.ElementAt(1).Id)), Times.Once);
        }

        public void Dispose()
        {
            _sharedContext.ResetState();
        }
    }
}