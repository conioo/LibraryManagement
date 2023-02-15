using Application.Services;
using ApplicationTests;

namespace Application.UnitTests
{
    public class ItemServiceTests :IDisposable //IClassFixture<ContextFixture>, IDisposable
    {
        //private readonly ContextFixture _context;

        //public ItemServiceTests(ContextFixture context)
        //{
        //    _context = context;


        //}

        public void Dispose()
        {
        }

        [Fact]
        void test1()
        {
            //var itemService = new ItemService(_context.InMemoryDbContext, _context.AutoMapper, _context.SieveProcessor);
            //itemService.GetAllAsync().Wait();
        }
       
    }
}
