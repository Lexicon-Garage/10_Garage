using Garage.Web.Data;
using Garage.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Garage.Test
{
    public class TestFixture
    {
        public AppDbConext Context { get; }
        public TestFixture()
        {
            var options = new DbContextOptionsBuilder<AppDbConext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new AppDbConext(options);
            TestDataSeeder.Seed(Context);
        }

      
    }
}