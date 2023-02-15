using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace Application.Sieve
{
    internal class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(IOptions<SieveOptions> options) : base(options) { }
        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            var assemblyIdentity = AppDomain.CurrentDomain.GetAssemblies().Single(x => x.FullName.Contains("Infrastructure.Identity"));
            var assemblyPersistence = AppDomain.CurrentDomain.GetAssemblies().Single(x => x.FullName.Contains("Infrastructure.Persistence"));

            mapper.ApplyConfigurationsFromAssembly(assemblyPersistence);
            mapper.ApplyConfigurationsFromAssembly(assemblyIdentity);

            return mapper;
        }
    }
}
