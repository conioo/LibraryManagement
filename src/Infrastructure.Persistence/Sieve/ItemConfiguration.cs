using Domain.Entities;
using Sieve.Services;

namespace Infrastructure.Persistence.Sieve.Configurations
{
    public class ItemConfiguration : ISieveConfiguration
    {
        public void Configure(SievePropertyMapper mapper)
        {
            mapper.Property<Item>(obj => obj.Publisher).CanFilter().CanSort();
            mapper.Property<Item>(obj => obj.Authors).CanFilter().CanSort();
            mapper.Property<Item>(obj => obj.FormOfPublication).CanFilter().CanSort().HasName("form");
            mapper.Property<Item>(obj => obj.Title).CanFilter().CanSort();
            mapper.Property<Item>(obj => obj.YearOfPublication).CanFilter().CanSort().HasName("year");

            mapper.Property<Item>(obj => obj.Description).CanFilter();
            mapper.Property<Item>(obj => obj.ISBN).CanFilter();
        }
    }
}
