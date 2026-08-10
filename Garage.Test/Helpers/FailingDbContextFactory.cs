using Garage.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Garage.Test.Helpers;

public class ThrowDbUpdateExceptionInterceptor: SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>>
        SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
    {
        throw new DbUpdateException(
            "Simulated database failure.");
    }
}

public class FailingDbContextFactory
{
    private readonly string _databaseName;

    public FailingDbContextFactory()
    {
        _databaseName = Guid.NewGuid().ToString();
    }

    public AppDbConext CreateNormalContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbConext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;

        return new AppDbConext(options);
    }

    public AppDbConext CreateFailingContext()
    {
        var interceptor =
            new ThrowDbUpdateExceptionInterceptor();

        var options =
            new DbContextOptionsBuilder<AppDbConext>()
                .UseInMemoryDatabase(_databaseName)
                .AddInterceptors(interceptor)
                .Options;

        return new AppDbConext(options);
    }
}