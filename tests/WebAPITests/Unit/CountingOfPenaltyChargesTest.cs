using Application.Interfaces;
using Application.Reactive.Observers;
using Domain.Interfaces;
using Domain.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace WebAPITests.Unit
{
    internal class CountingOfPenaltyChargesTest : CountingOfPenaltyCharges
    {
        private readonly IUnitOfWork _unitOfWork;

        public CountingOfPenaltyChargesTest(IServiceProvider serviceProvider, ILogger<CountingOfPenaltyCharges> logger, IOptions<RentalSettings> options, IEmailService emailService, IUnitOfWork unitOfWork) : base(serviceProvider, logger, options, emailService)
        {
            _unitOfWork = unitOfWork;
        }

        protected override TService GetService<TService>(IServiceProvider provider)
        {
            if(typeof(TService) == typeof(IUnitOfWork))
            {
                return (dynamic)_unitOfWork;
            }

            if (typeof(TService) == typeof(IUserService))
            {
                var userServiceMock = new Mock<IUserService>();
                userServiceMock.Setup(obj => obj.GetEmail(It.IsAny<string>())).Returns(Task.FromResult("mail@mail.com"));

                return (dynamic)userServiceMock.Object;
            }

            return base.GetService<TService>(provider);
        }

        protected override IServiceScope CreateScope(IServiceProvider serviceProvider)
        {
            var serviceScopeMock = new Mock<IServiceScope>();
            return serviceScopeMock.Object;
        }
    }
}
