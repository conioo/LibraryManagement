using WebAPI.Reactive;

namespace WebAPI.Extensions
{
    public static class WebApplicationExtension
    {
        public static void UseReactiveComponents(this WebApplication webApplication)
        {

            ReactiveManager.CreateDailyObservable(webApplication.Services);
        }

    }
}
