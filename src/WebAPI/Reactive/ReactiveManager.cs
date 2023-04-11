using Application.Reactive.Interfaces;
using Domain.Interfaces;
using System.Reactive.Linq;

namespace WebAPI.Reactive
{
    public static class ReactiveManager
    {
        public static void CreateDailyObservable(IServiceProvider serviceProvider)
        {
            IObservable<DateTimeOffset> daily = Observable.Create<DateTimeOffset>(observer =>
            {
                //var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                var userTimeZone = TimeZoneInfo.Local;

                //var startTime = DateTimeOffset.Now.AddSeconds(5);
                var startTime = new DateTimeOffset(DateTime.Today.Ticks, userTimeZone.GetUtcOffset(DateTime.Today));

                if (startTime < DateTimeOffset.Now)
                {
                    startTime = startTime.AddDays(1.0);
                }

                return Observable.Timer(startTime, TimeSpan.FromDays(1.0))
                                 .Select(val => DateTimeOffset.Now)
                                 .Subscribe(observer);
            });

            var endOfReservation = serviceProvider.GetRequiredService<IEndOfReservation>();
            var startingCountingOfPenaltyCharges = serviceProvider.GetRequiredService<IStartingCountingOfPenaltyCharges>();
            var countingOfPenaltyCharges = serviceProvider.GetRequiredService<ICountingOfPenaltyCharges>();

            daily.Subscribe(endOfReservation);
            daily.Subscribe(startingCountingOfPenaltyCharges);
            daily.Subscribe(countingOfPenaltyCharges);
        }
    }
}