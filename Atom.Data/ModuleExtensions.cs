using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Data.Persistence;
using Genius.Atom.Infrastructure.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Data;

[ExcludeFromCodeCoverage]
public static class ModuleExtensions
{
    public static void RegisterRepository<TEntity, TImplementation, TQueryServiceInterface, TRepositoryInterface>(this IServiceCollection services)
        where TEntity: EntityBase
        where TImplementation : RepositoryBase<TEntity>, TQueryServiceInterface, TRepositoryInterface
        where TQueryServiceInterface : class, IQueryService<TEntity>
        where TRepositoryInterface: class, IRepository<TEntity>
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TQueryServiceInterface>(sp => sp.GetRequiredService<TImplementation>());
        services.AddSingleton<TRepositoryInterface>(sp => sp.GetRequiredService<TImplementation>());
    }
}
