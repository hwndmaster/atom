using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Data.Persistence;

public interface ITypeDiscriminators
{
    void AddMapping<T>(string discriminator, int version = 1);
    void AddMapping<T, TPreviousVersion, TVersionUpgrader>(string discriminator, int version)
        where T : class
        where TPreviousVersion : class
        where TVersionUpgrader : IDataVersionUpgrader<TPreviousVersion, T>;
    bool HasMapping(Type type);
}

internal interface ITypeDiscriminatorsInternal : ITypeDiscriminators
{
    TypeDiscriminatorRecord GetDiscriminator(Type type);
    TypeDiscriminatorRecord GetDiscriminator(string discriminator, int version);
    TypeDiscriminatorRecord? GetDiscriminatorByPreviousVersion(Type type);
}

internal record TypeDiscriminatorRecord(string Discriminator, Type Type, Type? PreviousVersionType, int Version, DataVersionUpgraderProxy? VersionUpgrader);

internal sealed class TypeDiscriminators : ITypeDiscriminators, ITypeDiscriminatorsInternal
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<string, TypeDiscriminatorRecord> _discriminatorsByDiscriminatorAndVersion = new();
    private readonly Dictionary<string, TypeDiscriminatorRecord> _discriminatorsByTypeName = new();

    public TypeDiscriminators(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider.NotNull();
    }

    public void AddMapping<T>(string discriminator, int version = 1)
    {
        var record = new TypeDiscriminatorRecord(discriminator, typeof(T), null, version, null);
        AddMapping(record);
    }

    public void AddMapping<T, TPreviousVersion, TVersionUpgrader>(string discriminator, int version)
        where T : class
        where TPreviousVersion : class
        where TVersionUpgrader : IDataVersionUpgrader<TPreviousVersion, T>
    {
        var versionUpgrader = _serviceProvider.GetRequiredService<TVersionUpgrader>();
        var versionUpgraderProxy = DataVersionUpgraderProxy.Create(versionUpgrader);

        var previousVersionType = typeof(TPreviousVersion);
        if (!_discriminatorsByTypeName.TryGetValue(previousVersionType.FullName.NotNull(), out var previousVersionRecord))
        {
            throw new InvalidOperationException($"The discriminator for type '{previousVersionType.FullName}' is not registered.");
        }

        if (previousVersionRecord.Version >= version)
        {
            throw new InvalidOperationException($"The previous version '{previousVersionRecord.Version}' must be less than the passing one '{version}'.");
        }

        var record = new TypeDiscriminatorRecord(discriminator, typeof(T), previousVersionType, version, versionUpgraderProxy);
        AddMapping(record);
    }

    public bool HasMapping(Type type)
    {
        return _discriminatorsByTypeName.ContainsKey(type.FullName.NotNull());
    }

    public TypeDiscriminatorRecord GetDiscriminator(Type type)
    {
        Guard.NotNull(type);
        Guard.NotNull(type.FullName);

        if (!_discriminatorsByTypeName.TryGetValue(type.FullName, out var value))
        {
            throw new InvalidOperationException($"The discriminator for type '{type.FullName}' is not registered.");
        }

        return value;
    }

    public TypeDiscriminatorRecord GetDiscriminator(string discriminator, int version)
    {
        Guard.NotNull(discriminator);

        if (!_discriminatorsByDiscriminatorAndVersion.TryGetValue(CreateDiscriminatorQualifiedName(discriminator, version), out var value))
        {
            throw new InvalidOperationException($"The discriminator named '{discriminator}' is not registered with version {version}.");
        }

        return value;
    }

    public TypeDiscriminatorRecord? GetDiscriminatorByPreviousVersion(Type type)
    {
        Guard.NotNull(type);

        return _discriminatorsByTypeName.Values.FirstOrDefault(x => x.PreviousVersionType == type);
    }

    /// <summary>
    ///   Only for testing purposes. Shouldn't be used.
    /// </summary>
    /// <param name="type"></param>
    internal void RemoveMapping(Type type)
    {
        var record = _discriminatorsByTypeName[type.FullName!];
        _discriminatorsByTypeName.Remove(type.FullName!);
        _discriminatorsByDiscriminatorAndVersion.Remove(CreateDiscriminatorQualifiedName(record.Discriminator, record.Version));
    }

    private void AddMapping(TypeDiscriminatorRecord record)
    {
        _discriminatorsByDiscriminatorAndVersion.Add(CreateDiscriminatorQualifiedName(record.Discriminator, record.Version), record);
        _discriminatorsByTypeName.Add(record.Type.FullName.NotNull(), record);
    }

    private static string CreateDiscriminatorQualifiedName(string discriminator, int version)
    {
        return $"{discriminator};{version}";
    }
}
