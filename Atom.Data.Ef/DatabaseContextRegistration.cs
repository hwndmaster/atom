using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Data.Ef;

/// <summary>
/// Provides a method to register the database context with the dependency injection container.
/// </summary>
public static class DatabaseContextRegistration
{
    public static void Register<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IDatabaseContext, DatabaseContext<TDbContext>>();
    }
}
