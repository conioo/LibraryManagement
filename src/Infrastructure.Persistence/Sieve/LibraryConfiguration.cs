using Domain.Entities;
using Sieve.Services;

namespace Infrastructure.Persistence.Sieve
{
    public class LibraryConfiguration : ISieveConfiguration
    {
        public void Configure(SievePropertyMapper mapper)
        {
            mapper.Property<Library>(model => model.Name).CanFilter().CanSort();
            mapper.Property<Library>(model => model.Description).CanFilter();
            mapper.Property<Library>(model => model.Address).CanFilter();
            mapper.Property<Library>(model => model.NumberOfComputerStations).CanFilter().CanSort().HasName("computers");
            mapper.Property<Library>(model => model.IsScanner).CanFilter().CanSort();
            mapper.Property<Library>(model => model.IsPrinter).CanFilter().CanSort();
            mapper.Property<Library>(model => model.IsPhotocopier).CanFilter().CanSort();
        }
    }
}
