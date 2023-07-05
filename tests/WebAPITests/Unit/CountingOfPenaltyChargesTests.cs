using Application.Dtos;
using Application.Interfaces;
using Application.Reactive.Observers;
using CommonContext;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using FluentAssertions;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace WebAPITests.Unit
{
    public class CountingOfPenaltyChargesTests
    {
        private CountingOfPenaltyChargesTest _countingOfPenaltyChargesTest;
        private readonly Rental _rental;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<CountingOfPenaltyCharges>> _loggerMock;
        private readonly IUnitOfWork _unitOfWork;

        public CountingOfPenaltyChargesTests()
        {
            var userResolverServiceMock = new Mock<IUserResolverService>();
            userResolverServiceMock.Setup(obj => obj.GetUserName).Returns("counting_test_username");

            _unitOfWork = new LibraryDbContext(new DbContextOptionsBuilder<LibraryDbContext>().UseInMemoryDatabase("counting_librarydb").EnableSensitiveDataLogging().Options, null, userResolverServiceMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();

            //var rentalSettingsMock = new Mock<RentalSettings>();

            //rentalSettingsMock.SetupProperty(obj => obj.PenaltyChargePerDay, 1);
            //rentalSettingsMock.SetupProperty(obj => obj.TimeInDays, 30);

            var rentalOptionsMock = new Mock<IOptions<RentalSettings>>();

            rentalOptionsMock.SetupProperty(obj => obj.Value.PenaltyChargePerDay, 1);
            rentalOptionsMock.SetupProperty(obj => obj.Value.TimeInDays, 30);

            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<CountingOfPenaltyCharges>>();


            _countingOfPenaltyChargesTest = new CountingOfPenaltyChargesTest(serviceProviderMock.Object, _loggerMock.Object, rentalOptionsMock.Object, _emailServiceMock.Object, _unitOfWork);

            _rental = DataGenerator.GetOneWithDependencies<Rental>();

            _rental.BeginDate = DateOnly.FromDateTime(DateTime.Now);
            _rental.EndDate = _rental.BeginDate.AddDays(30);

            _unitOfWork.Set<Rental>().Add(_rental);
            _unitOfWork.SaveChanges();
        }

        [Fact]
        public Task AddRental_After30DaysStartsToCharge_SendsAnEmailInforming()
        {
            _countingOfPenaltyChargesTest.AddRental(_rental);

            _countingOfPenaltyChargesTest.OnNext(DateTimeOffset.Now.AddDays(30));

            _emailServiceMock.Verify(obj => obj.SendAsync(It.Is<Email>(email => email.To == "mail@mail.com")), Times.Once);

            var updatedRental = _unitOfWork.Set<Rental>().Find(_rental.Id);

            updatedRental.Should().NotBeNull();
            updatedRental.PenaltyCharge.Should().Be(1);

            return Task.FromResult(0);
        }

        [Fact]
        public Task AddRental_After29DaysStartsToCharge_DoesNothing()
        {
            _countingOfPenaltyChargesTest.AddRental(_rental);

            _countingOfPenaltyChargesTest.OnNext(DateTimeOffset.Now.AddDays(29));

            _emailServiceMock.Verify(obj => obj.SendAsync(It.Is<Email>(email => email.To == "mail@mail.com")), Times.Never);

            var updatedRental = _unitOfWork.Set<Rental>().Find(_rental.Id);

            updatedRental.Should().NotBeNull();
            updatedRental.PenaltyCharge.Should().BeNull();

            return Task.FromResult(0);
        }

        [Fact]
        public Task ReturnOfItem_OnTime_ReturnsTrue()
        {
            _countingOfPenaltyChargesTest.AddRental(_rental);
            _countingOfPenaltyChargesTest.OnNext(DateTimeOffset.Now.AddDays(20));

            _unitOfWork.ChangeTracker.Clear();

            var renewRental = _unitOfWork.Set<Rental>().Find(_rental.Id);

            _countingOfPenaltyChargesTest.ReturnOfItem(renewRental).Should().BeTrue();

            return Task.FromResult(0);
        }

        [Fact]
        public Task ReturnOfItem_AfterTime_ReturnsTrue()
        {
            _countingOfPenaltyChargesTest.AddRental(_rental);
            _countingOfPenaltyChargesTest.OnNext(DateTimeOffset.Now.AddDays(30));

            _unitOfWork.ChangeTracker.Clear();

            var renewRental = _unitOfWork.Set<Rental>().Find(_rental.Id);

            _countingOfPenaltyChargesTest.ReturnOfItem(renewRental).Should().BeTrue();

            return Task.FromResult(0);
        }

        [Fact]
        public Task ReturnOfItem_ForUnregisteredRental_ReturnsFalse()
        {
            _countingOfPenaltyChargesTest.ReturnOfItem(_rental).Should().BeFalse();

            return Task.FromResult(0);
        }

        [Fact]
        public Task RenewalRental_ForValidRentalIdAndDates_ReturnsTrue()
        {
            _countingOfPenaltyChargesTest.AddRental(_rental);

            _countingOfPenaltyChargesTest.RenewalRental(_rental.Id, _rental.EndDate, _rental.EndDate.AddDays(30)).Should().BeTrue();

            _countingOfPenaltyChargesTest.OnNext(DateTimeOffset.Now.AddDays(60));

            _emailServiceMock.Verify(obj => obj.SendAsync(It.Is<Email>(email => email.To == "mail@mail.com")), Times.Once);
            var updatedRental = _unitOfWork.Set<Rental>().Find(_rental.Id);

            updatedRental.Should().NotBeNull();
            updatedRental.PenaltyCharge.Should().Be(1);

            return Task.FromResult(0);
        }

        [Fact]
        public Task RenewalRental_ForValidRentalIdButInvalidDate_ReturnsFalse()
        {
            _countingOfPenaltyChargesTest.AddRental(_rental);

            _countingOfPenaltyChargesTest.RenewalRental(_rental.Id, _rental.EndDate.AddDays(5), _rental.EndDate.AddDays(30)).Should().BeFalse();

            _countingOfPenaltyChargesTest.OnNext(DateTimeOffset.Now.AddDays(60));

            _emailServiceMock.Verify(obj => obj.SendAsync(It.Is<Email>(email => email.To == "mail@mail.com")), Times.Never);
            var updatedRental = _unitOfWork.Set<Rental>().Find(_rental.Id);

            updatedRental.Should().NotBeNull();
            updatedRental.PenaltyCharge.Should().BeNull();

            return Task.FromResult(0);
        }

        [Fact]
        public Task RenewalRental_ForInvalidRentalId_ReturnsFalse()
        {
            _countingOfPenaltyChargesTest.RenewalRental(_rental.Id, _rental.EndDate, _rental.EndDate.AddDays(30)).Should().BeFalse();

            _countingOfPenaltyChargesTest.OnNext(DateTimeOffset.Now.AddDays(60));

            _emailServiceMock.Verify(obj => obj.SendAsync(It.Is<Email>(email => email.To == "mail@mail.com")), Times.Never);
            var updatedRental = _unitOfWork.Set<Rental>().Find(_rental.Id);

            updatedRental.Should().NotBeNull();
            updatedRental.PenaltyCharge.Should().BeNull();

            return Task.FromResult(0);
        }
    }
}
