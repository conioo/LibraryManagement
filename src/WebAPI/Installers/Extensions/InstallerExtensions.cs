using System.Reflection;
using WebAPI.Installers.Interfaces;

namespace WebAPI.Installers.Extensions
{
    public static class InstallerExtension
    {
        public static void InstallServicesInAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            var installers = Assembly.GetExecutingAssembly()
                .ExportedTypes
                .Where(type => typeof(IInstaller).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .Select(Activator.CreateInstance).Cast<IInstaller>().ToList();

            foreach (var installer in installers)
            {
                installer.InstallServices(services, configuration);
            }
        }
    }
}
