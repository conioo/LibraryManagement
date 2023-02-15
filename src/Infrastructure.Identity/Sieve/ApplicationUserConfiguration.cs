using Infrastructure.Identity.Entities;
using Sieve.Services;

namespace Application.Sieve.Configurations
{
    public class ApplicationUserConfiguration : ISieveConfiguration
    {
        public void Configure(SievePropertyMapper mapper)
        {
            mapper.Property<ApplicationUser>(obj => obj.FirstName).CanFilter().CanSort();
            mapper.Property<ApplicationUser>(obj => obj.UserName).CanFilter().CanSort();
            mapper.Property<ApplicationUser>(obj => obj.Email).CanFilter().CanSort();
            mapper.Property<ApplicationUser>(obj => obj.EmailConfirmed).CanFilter().CanSort();
            mapper.Property<ApplicationUser>(obj => obj.LastName).CanFilter().CanSort();
        }
    }
}
