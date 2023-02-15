using Microsoft.AspNetCore.Identity;
using Sieve.Services;

namespace Infrastructure.Identity.Sieve
{
    public class IdentityRoleConfiguration : ISieveConfiguration
    {
        public void Configure(SievePropertyMapper mapper)
        {
            mapper.Property<IdentityRole>(obj => obj.Name).CanFilter().CanSort();
        }
    }
}
