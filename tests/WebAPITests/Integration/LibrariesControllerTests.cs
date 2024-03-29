﻿using Application.Dtos.Request;
using Application.Dtos.Response;
using CommonContext;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Sieve.Models;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;
using WebAPITests.Integration.SharedContextBuilders;

namespace WebAPITests.Integration
{
    public class LibrariesControllerTests : IClassFixture<LibraryContextBuilder>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly List<Library> _libraries;
        private readonly SharedContext _sharedContext;

        public LibrariesControllerTests(LibraryContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _libraries = (List<Library>)DataGenerator.Get<Library>(4);

            _sharedContext.DbContext.Set<Library>().AddRange(_libraries);

            _sharedContext.DbContext.SaveChangesAsync().Wait();
        }

        [Fact]
        async Task AddLibraryAsync_ForInvalidModel_Returns400BadRequest()
        {
            var LibraryRequest = DataGenerator.GetRequest<LibraryRequest>(1).First();

            LibraryRequest.Name = "";
            LibraryRequest.Address = "";
            LibraryRequest.PhoneNumber = "";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Libraries.AddLibrary),
                Content = JsonContent.Create(LibraryRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(3);
            _sharedContext.DbContext.Set<Library>().Count().Should().Be(4);
        }

        [Fact]
        async Task AddLibraryAsync_ForValidModel_Returns201Created()
        {
            var LibraryRequest = DataGenerator.GetRequest<LibraryRequest>(1).First();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Libraries.AddLibrary),
                Content = JsonContent.Create(LibraryRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            var responseLibrary = await response.Content.ReadFromJsonAsync<LibraryResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Libraries.Prefix}/{Libraries.GetLibraryById}?id={responseLibrary.Id}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().Be(expectedLocationUri);
            _sharedContext.DbContext.Set<Library>().Count().Should().Be(5);

            var newLibrary = await _sharedContext.DbContext.Set<Library>().FindAsync(responseLibrary.Id);

            newLibrary.CreatedBy.Should().Be("default");
            newLibrary.LastModifiedBy.Should().Be("default");

            newLibrary.Should().BeEquivalentTo(LibraryRequest, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetAllLibrariesAsync_ForFourLibraries_ReturnsAllLibraries()
        {
            var response = await _client.GetAsync(Libraries.GetAllLibraries);
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<LibraryResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.Should().BeEquivalentTo(_libraries, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetLibraryByIdAsync_ForInvalidId_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Libraries.GetLibraryById, "id", "null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetLibraryByIdAsync_ForValidId_ReturnsCorrectLibrary()
        {
            var requestUri = QueryHelpers.AddQueryString(Libraries.GetLibraryById, "id", _libraries.First().Id);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<LibraryResponse>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.Should().BeEquivalentTo(_libraries.First(), options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetPageAsync_ForInvalidSieveModel_Returns404BadRequest()
        {
            var sieveModel = new SieveModel();

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Libraries.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(2);
        }

        [Fact]
        async Task GetPageAsync_ForNonExistingPage_ReturnsEmptyPage()
        {
            var sieveModel = new SieveModel()
            {
                PageSize = 1,
                Page = 10,
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Libraries.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<LibraryResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.TotalPages.Should().Be(4);
            result.TotalItemsCount.Should().Be(4);
            result.Items.Should().BeEmpty();
        }

        [Fact]
        async Task GetPageAsync_ForValidPage_ReturnsCorrectPage()
        {
            var sieveModel = new SieveModel()
            {
                PageSize = 2,
                Page = 2,
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Libraries.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<LibraryResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.TotalPages.Should().Be(2);
            result.TotalItemsCount.Should().Be(4);

            _libraries.Should().ContainEquivalentOf(result.Items.ElementAt(0));
            _libraries.Should().ContainEquivalentOf(result.Items.ElementAt(1));
        }

        [Fact]
        async Task GetPageAsync_UsingSortedPageByComputers_ReturnsCorrectSortedPage()
        {
            var sieveModel = new SieveModel()
            {
                Page = 1,
                PageSize = 3,
                Sorts = "computers"
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Libraries.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<LibraryResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            result.Items.Count().Should().Be(3);

            result.Items.Should().BeInAscendingOrder(Library => Library.NumberOfComputerStations);
        }

        [Fact]
        async Task RemoveLibraryAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Libraries.RemoveLibrary + $"?id=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.DbContext.Set<Library>().Count().Should().Be(4);
        }

        [Fact]
        async Task RemoveLibraryAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Libraries.RemoveLibrary + $"?id={_libraries.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Library>().Count().Should().Be(3);
        }

        [Fact]
        async Task UpdateLibraryAsync_ForInvalidModel_Returns400BadRequest()
        {
            var updateLibrary = new LibraryRequest()
            {
                Name = "",
                NumberOfComputerStations = -1
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Libraries.UpdateLibrary + $"?id=null_null"),
                Content = JsonContent.Create(updateLibrary)
            };

            var response = await _client.SendAsync(request);

            var dbLibrary = _sharedContext.DbContext.Set<Library>().Find(_libraries.First().Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(2);

            _sharedContext.DbContext.Set<Library>().Count().Should().Be(4);
            dbLibrary.LastModified.Should().Be(dbLibrary.Created);
        }

        [Fact]
        async Task UpdateLibraryAsync_ForValidModel_Returns200Ok()
        {
            var updateLibrary = new LibraryRequest()
            {
                Name = "updated name",
                NumberOfComputerStations = 6,
                Description = "updated des"
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Libraries.UpdateLibrary + $"?id={_libraries.First().Id}"),
                Content = JsonContent.Create(updateLibrary)
            };

            _sharedContext.RefreshDb();

            var response = await _client.SendAsync(request);

            var dbLibrary = _sharedContext.DbContext.Set<Library>().Find(_libraries.First().Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Library>().Count().Should().Be(4);
            dbLibrary.LastModified.Should().BeAfter(dbLibrary.Created);

            dbLibrary.Should().BeEquivalentTo(_libraries.First(), opt => opt.Excluding(library => library.Description).Excluding(library => library.Name).Excluding(library => library.NumberOfComputerStations).Excluding(library => library.LastModified).Excluding(library => library.LastModifiedBy));
            dbLibrary.Name.Should().Be(updateLibrary.Name);
            dbLibrary.NumberOfComputerStations.Should().Be(updateLibrary.NumberOfComputerStations);
            dbLibrary.Description.Should().Be(updateLibrary.Description);
        }

        public void Dispose()
        {
            _sharedContext.ResetState();
        }
    }
}
