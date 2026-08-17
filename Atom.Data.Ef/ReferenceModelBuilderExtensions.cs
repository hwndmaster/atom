using System.Linq.Expressions;
using Genius.Atom.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Genius.Atom.Data.Ef;

/// <summary>
/// Extension methods for mapping strongly-typed <see cref="IReference{TKey, TReference}"/> properties
/// onto their underlying key values in the database.
/// </summary>
/// <remarks>
/// The overloads are resolved by the key type of <typeparamref name="TReference"/>: <see cref="int"/>-keyed
/// references default to a database-generated identifier, <see cref="Guid"/>-keyed ones to a client-generated
/// identifier. Those are conventions only, not a property of the key type itself, so pass
/// <see cref="ReferenceIdGeneration"/> explicitly whenever an entity deviates from them.
/// </remarks>
public static class ReferenceModelBuilderExtensions
{
    /// <summary>
    /// Configures a value conversion for the <see cref="EntityBase{TKey, TReference}.Id"/> property
    /// of <typeparamref name="TEntity"/>, mapping between the strongly-typed <typeparamref name="TReference"/>
    /// and an <see cref="int"/> in the database.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TReference">The strongly-typed reference used as the entity's identifier.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="fromProvider">Creates a <typeparamref name="TReference"/> from a key value read from the database.</param>
    /// <param name="idGeneration">Who assigns the identifier on insert. Defaults to <see cref="ReferenceIdGeneration.Database"/>.</param>
    /// <returns>The same <paramref name="modelBuilder"/> instance, so that calls can be chained.</returns>
    public static ModelBuilder ConfigureReferenceId<TEntity, TReference>(this ModelBuilder modelBuilder,
        Expression<Func<int, TReference>> fromProvider,
        ReferenceIdGeneration idGeneration = ReferenceIdGeneration.Database)
        where TEntity : EntityBase<int, TReference>
        where TReference : IReference<int, TReference>
        => ConfigureReferenceIdCore<TEntity, int, TReference>(modelBuilder, fromProvider, idGeneration);

    /// <summary>
    /// Configures a value conversion for the <see cref="EntityBase{TKey, TReference}.Id"/> property
    /// of <typeparamref name="TEntity"/>, mapping between the strongly-typed <typeparamref name="TReference"/>
    /// and a <see cref="Guid"/> in the database.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TReference">The strongly-typed reference used as the entity's identifier.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="fromProvider">Creates a <typeparamref name="TReference"/> from a key value read from the database.</param>
    /// <param name="idGeneration">Who assigns the identifier on insert. Defaults to <see cref="ReferenceIdGeneration.Client"/>.</param>
    /// <returns>The same <paramref name="modelBuilder"/> instance, so that calls can be chained.</returns>
    public static ModelBuilder ConfigureReferenceId<TEntity, TReference>(this ModelBuilder modelBuilder,
        Expression<Func<Guid, TReference>> fromProvider,
        ReferenceIdGeneration idGeneration = ReferenceIdGeneration.Client)
        where TEntity : EntityBase<Guid, TReference>
        where TReference : IReference<Guid, TReference>
        => ConfigureReferenceIdCore<TEntity, Guid, TReference>(modelBuilder, fromProvider, idGeneration);

    /// <summary>
    /// Configures a value conversion for a foreign key property of type <typeparamref name="TReference"/>
    /// on <typeparamref name="TEntity"/>, mapping between the strongly-typed reference and an
    /// <see cref="int"/> in the database.
    /// </summary>
    /// <typeparam name="TEntity">The entity type declaring the foreign key property.</typeparam>
    /// <typeparam name="TReference">The strongly-typed reference the foreign key points at.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="propertyName">The name of the foreign key property.</param>
    /// <param name="fromProvider">Creates a <typeparamref name="TReference"/> from a key value read from the database.</param>
    /// <returns>The same <paramref name="modelBuilder"/> instance, so that calls can be chained.</returns>
    public static ModelBuilder ConfigureReferenceFk<TEntity, TReference>(this ModelBuilder modelBuilder,
        string propertyName, Expression<Func<int, TReference>> fromProvider)
        where TEntity : class
        where TReference : IReference<int, TReference>
        => ConfigureReferenceFkCore<TEntity, int, TReference>(modelBuilder, propertyName, fromProvider);

    /// <summary>
    /// Configures a value conversion for a foreign key property of type <typeparamref name="TReference"/>
    /// on <typeparamref name="TEntity"/>, mapping between the strongly-typed reference and a
    /// <see cref="Guid"/> in the database.
    /// </summary>
    /// <typeparam name="TEntity">The entity type declaring the foreign key property.</typeparam>
    /// <typeparam name="TReference">The strongly-typed reference the foreign key points at.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="propertyName">The name of the foreign key property.</param>
    /// <param name="fromProvider">Creates a <typeparamref name="TReference"/> from a key value read from the database.</param>
    /// <returns>The same <paramref name="modelBuilder"/> instance, so that calls can be chained.</returns>
    public static ModelBuilder ConfigureReferenceFk<TEntity, TReference>(this ModelBuilder modelBuilder,
        string propertyName, Expression<Func<Guid, TReference>> fromProvider)
        where TEntity : class
        where TReference : IReference<Guid, TReference>
        => ConfigureReferenceFkCore<TEntity, Guid, TReference>(modelBuilder, propertyName, fromProvider);

    private static ModelBuilder ConfigureReferenceIdCore<TEntity, TKey, TReference>(ModelBuilder modelBuilder,
        Expression<Func<TKey, TReference>> fromProvider,
        ReferenceIdGeneration idGeneration)
        where TKey : notnull
        where TEntity : EntityBase<TKey, TReference>
        where TReference : IReference<TKey, TReference>
    {
        modelBuilder.NotNull();
        fromProvider.NotNull();

        var property = modelBuilder.Entity<TEntity>()
            .Property(e => e.Id)
            .HasConversion(
                r => r.Id,
                fromProvider);

        if (idGeneration == ReferenceIdGeneration.Database)
        {
            property
                .ValueGeneratedOnAdd()
                .HasSentinel(TReference.Create(default!));
        }
        else
        {
            property.ValueGeneratedNever();
        }

        return modelBuilder;
    }

    private static ModelBuilder ConfigureReferenceFkCore<TEntity, TKey, TReference>(ModelBuilder modelBuilder,
        string propertyName, Expression<Func<TKey, TReference>> fromProvider)
        where TKey : notnull
        where TEntity : class
        where TReference : IReference<TKey, TReference>
    {
        modelBuilder.NotNull();
        propertyName.NotNull();
        fromProvider.NotNull();

        modelBuilder.Entity<TEntity>()
            .Property<TReference>(propertyName)
            .HasConversion(
                r => r.Id,
                fromProvider);

        return modelBuilder;
    }
}
