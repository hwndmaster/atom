using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Data.JsonPersistence;
using Genius.Atom.Infrastructure.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Data;

[ExcludeFromCodeCoverage]
public static class ModuleExtensions
{
    public static void RegisterJsonRepository<TKey, TReference, TEntity, TImplementation, TQueryServiceInterface, TRepositoryInterface>(this IServiceCollection services)
        where TKey : notnull
        where TReference : IReference<TKey, TReference>
        where TEntity : EntityBase<TKey, TReference>
        where TImplementation : JsonRepositoryBase<TKey, TReference, TEntity>, TQueryServiceInterface, TRepositoryInterface
        where TQueryServiceInterface : class, IQueryService<TEntity>
        where TRepositoryInterface: class, IJsonRepository<TKey, TReference, TEntity>
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TQueryServiceInterface>(sp => sp.GetRequiredService<TImplementation>());
        services.AddSingleton<TRepositoryInterface>(sp => sp.GetRequiredService<TImplementation>());
    }
}
