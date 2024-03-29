﻿using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;
using CommonContext;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;
using WebAPITests.Integration.SharedContextBuilders;
using Profile = Domain.Entities.Profile;

namespace WebAPITests.Integration
{
    public class CopiesControllerTests : IClassFixture<CopiesContextBuilder>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly List<Copy> _copies;
        private readonly int _defaultNumberCopies;
        private readonly Item _item;
        private readonly Library _library;
        private readonly SharedContext _sharedContext;

        public CopiesControllerTests(CopiesContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _library = DataGenerator.Get<Library>(1).First();
            _item = DataGenerator.Get<Item>(1).First();

            _sharedContext.DbContext.Set<Library>().Add(_library);
            _sharedContext.DbContext.Set<Item>().Add(_item);

            _defaultNumberCopies = 2;

            var profile = DataGenerator.Get<Profile>(1).First();

            _copies = new List<Copy>() { DataGenerator.GetOne<Copy>() };
            _copies.First().IsAvailable = false;

            var rental = DataGenerator.GetOne<Rental>();
            rental.Copy = _copies.First();
            rental.Profile = profile;

            var reservation = DataGenerator.GetOne<Domain.Entities.Reservation>();
            reservation.Copy = _copies.First();
            reservation.Profile = profile;

            _copies.First().CurrentRental = rental;
            _copies.First().CurrentReservation = reservation;
            _copies.First().Item = _item;
            _copies.First().Library = _library;
            _copies.First().CopyHistory = DataGenerator.GetOneWithDependencies<CopyHistory>();
            _copies.First().CopyHistory.ArchivalRentals.First().ProfileHistory = profile.ProfileHistory;
            _copies.First().CopyHistory.ArchivalReservations.First().ProfileHistory = profile.ProfileHistory;

            _copies.Add(DataGenerator.GetOneWithDependencies<Copy>());

            _sharedContext.DbContext.Set<Copy>().AddRange(_copies);

            _sharedContext.DbContext.SaveChangesAsync().Wait();
        }

        [Fact]
        async Task AddCopiesAsync_ForInvalidItemId_Returns404NotFound()
        {
            var copyRequest = new CopyRequest()
            {
                ItemId = "nullnull",
                LibraryId = _library.Id,
                Count = 3
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Copies.AddCopies),
                Content = JsonContent.Create(copyRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

        [Fact]
        async Task AddCopiesAsync_ForInvalidLibraryId_Returns404NotFound()
        {
            var copyRequest = new CopyRequest()
            {
                ItemId = _item.Id,
                LibraryId = "nullnull",
                Count = 3
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Copies.AddCopies),
                Content = JsonContent.Create(copyRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

        [Fact]
        async Task AddCopiesAsync_ForInvalidModel_Returns400BadRequest()
        {
            var copyRequest = new CopyRequest()
            {
                Count = 0,
                ItemId = null,
                LibraryId = "exist"
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Copies.AddCopies),
                Content = JsonContent.Create(copyRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(2);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

        [Fact]
        async Task AddCopiesAsync_ForValidModel_Returns200Ok()
        {
            var copyHistoryNumber = _sharedContext.DbContext.Set<CopyHistory>().Count();

            var copyRequest = new CopyRequest()
            {
                ItemId = _item.Id,
                LibraryId = _library.Id,
                Count = 3
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Copies.AddCopies),
                Content = JsonContent.Create(copyRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies + 3);
            _sharedContext.DbContext.Set<CopyHistory>().Count().Should().Be(copyHistoryNumber + 3);

            var newCopy = _sharedContext.DbContext.Set<Copy>().First(copy => copy.CreatedBy == "default");

            newCopy.CreatedBy.Should().Be("default");
            newCopy.LastModifiedBy.Should().Be("default");

            newCopy.Created.Should().NotBe(null);
            newCopy.LastModified.Should().NotBe(null);

            newCopy.Should().BeEquivalentTo(copyRequest, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetCopyByIdAsync_ForInvalidInventoryNumber_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.GetCopyById, "inventory-number", "null_null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetCopyByIdAsync_ForValidInventoryNumber_Returns200Ok()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.GetCopyById, "inventory-number", _copies.First().InventoryNumber);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var copyResponse = await response.Content.ReadFromJsonAsync<CopyResponse>();

            copyResponse.Should().BeEquivalentTo(_copies.First(), options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetCurrentRentalAsync_ForInvalidInventoryNumber_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.GetCurrentRental, "inventory-number", "null_null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetCurrentRentalAsync_ForNoRental_Returns204NoContent()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.GetCurrentRental, "inventory-number", _copies.Last().InventoryNumber);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [Fact]
        async Task GetCurrentRentalAsync_ForValidInventoryNumber_Returns200Ok()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.GetCurrentRental, "inventory-number", _copies.First().InventoryNumber);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var rentalResponse = await response.Content.ReadFromJsonAsync<RentalResponse?>();

            rentalResponse.Should().BeEquivalentTo(_copies.First().CurrentRental, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetCurrentReservationAsync_ForInvalidInventoryNumber_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.GetCurrentReservation, "inventory-number", "null_null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetCurrentReservationAsync_ForNoRental_Returns204NoContent()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.GetCurrentReservation, "inventory-number", _copies.Last().InventoryNumber);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [Fact]
        async Task GetCurrentReservationAsync_ForValidInventoryNumber_Returns200Ok()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.GetCurrentReservation, "inventory-number", _copies.First().InventoryNumber);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var reservationResponse = await response.Content.ReadFromJsonAsync<ReservationResponse?>();

            reservationResponse.Should().BeEquivalentTo(_copies.First().CurrentReservation, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetHistoryByInventoryNumberAsync_ForInvalidInventoryNumber_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Copies.GetHistoryByInventoryNumber + $"?inventory-number=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetHistoryByInventoryNumberAsync_ForValidInventoryNumber_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Copies.GetHistoryByInventoryNumber + $"?inventory-number={_copies.First().InventoryNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var copyResponse = await response.Content.ReadFromJsonAsync<CopyHistoryResponse>();

            copyResponse.Should().BeEquivalentTo(_copies.First().CopyHistory, options =>
            {
                options.AllowingInfiniteRecursion();
                options.ExcludingMissingMembers();
                return options;
            });

            copyResponse.ArchivalRentals.First().ItemTitle.Should().Be(_copies.First().Item.Title);
            copyResponse.ArchivalRentals.First().ProfileLibraryCardNumber.Should().Be(_copies.First().CopyHistory.ArchivalRentals.First().ProfileHistory.Profile.LibraryCardNumber);
            copyResponse.ArchivalRentals.First().CopyInventoryNumber.Should().Be(_copies.First().CopyHistory.ArchivalRentals.First().CopyHistory.Copy.InventoryNumber);

            copyResponse.ArchivalReservations.First().ItemTitle.Should().Be(_copies.First().Item.Title);
            copyResponse.ArchivalReservations.First().ProfileLibraryCardNumber.Should().Be(_copies.First().CopyHistory.ArchivalReservations.First().ProfileHistory.Profile.LibraryCardNumber);
            copyResponse.ArchivalReservations.First().CopyInventoryNumber.Should().Be(_copies.First().CopyHistory.ArchivalReservations.First().CopyHistory.Copy.InventoryNumber);
        }

        [Fact]
        async Task IsAvailableAsync_ForAvailableCopy_ReturnsTrue()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.IsAvailable, "inventory-number", _copies.Last().InventoryNumber);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var isAvailable = await response.Content.ReadFromJsonAsync<bool>();

            isAvailable.Should().BeTrue();
        }

        [Fact]
        async Task IsAvailableAsync_ForInvalidInventoryNumber_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.IsAvailable, "inventory-number", "null_null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task IsAvailableAsync_ForUnavailableCopy_ReturnsFalse()
        {
            var requestUri = QueryHelpers.AddQueryString(Copies.IsAvailable, "inventory-number", _copies.First().InventoryNumber);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var isAvailable = await response.Content.ReadFromJsonAsync<bool>();

            isAvailable.Should().BeFalse();
        }

        [Fact]
        async Task RemoveCopiesAsync_ForValidInventoryNumbers_Returns200Ok()
        {
            List<string> inventoryNumbers = new List<string>
            {
                _copies[0].InventoryNumber,
                _copies[1].InventoryNumber,
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Copies.RemoveCopies),
                Content = JsonContent.Create(inventoryNumbers)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(0);
        }

        [Fact]
        async Task RemoveCopiesAsync_OneInValidInventoryNumber_Returns404NotFound()
        {
            List<string> inventoryNumbers = new List<string>
            {
                _copies[0].InventoryNumber,
                "null_null",
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Copies.RemoveCopies),
                Content = JsonContent.Create(inventoryNumbers)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

        [Fact]
        async Task RemoveCopyAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Copies.RemoveCopy + $"?inventory-number=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

        [Fact]
        async Task RemoveCopyAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Copies.RemoveCopy + $"?inventory-number={_copies.First().InventoryNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies - 1);
        }

        public void Dispose()
        {
            _sharedContext.ResetState();
        }
    }
}
