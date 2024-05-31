using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Data.Persistence;

public interface ITypeDiscriminators
{
    void AddMapping<T>(string discriminator, int version = 1);
    void AddMapping<T, TPreviousVersion, TVersionUpgrader>(string discriminator, int version)
        where T : class
        where TPreviousVersion : class
        where TVersionUpgrader : IDataVersionUpgrader<TPreviousVersion, T>;
    void AddVersionUpgrader<T, TPreviousVersion, TVersionUpgrader>()
        where T : class
        where TPreviousVersion : class
        where TVersionUpgrader : IDataVersionUpgrader<TPreviousVersion, T>;
    bool HasMapping(Type type);
}

internal interface ITypeDiscriminatorsInternal : ITypeDiscriminators
{
    TypeDiscriminatorRecord GetDiscriminator(Type type);
    TypeDiscriminatorRecord GetDiscriminator(string discriminator, int version);
    (TypeDiscriminatorRecord Record, TypePreviousVersion PreviousVersion)? GetDiscriminatorByPreviousVersion(Type type);
}

internal sealed record TypePreviousVersion(Type PreviousVersionType, DataVersionUpgraderProxy VersionUpgrader);
internal sealed record TypeDiscriminatorRecord(string Discriminator, Type Type, List<TypePreviousVersion> PreviousVersions, int Version);

internal sealed class TypeDiscriminators : ITypeDiscriminatorsInternal
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TypeDiscriminators> _logger;

    private readonly Dictionary<string, TypeDiscriminatorRecord> _discriminatorsByDiscriminatorAndVersion = new();
    private readonly Dictionary<string, TypeDiscriminatorRecord> _discriminatorsByTypeName = new();
    private readonly Dictionary<string, List<TypeDiscriminatorRecord>> _discriminatorsByBaseTypeName = new();

    public TypeDiscriminators(IServiceProvider serviceProvider, ILogger<TypeDiscriminators> logger)
    {
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public void AddMapping<T>(string discriminator, int version = 1)
    {
        var record = new TypeDiscriminatorRecord(discriminator, typeof(T), [], version);
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

        var record = new TypeDiscriminatorRecord(discriminator, typeof(T), [new TypePreviousVersion(previousVersionType, versionUpgraderProxy)], version);
        AddMapping(record);
    }

    public void AddVersionUpgrader<T, TPreviousVersion, TVersionUpgrader>()
        where T : class
        where TPreviousVersion : class
        where TVersionUpgrader : IDataVersionUpgrader<TPreviousVersion, T>
    {
        var previousVersionType = typeof(TPreviousVersion);
        if (!_discriminatorsByTypeName.TryGetValue(previousVersionType.FullName.NotNull(), out var previousVersionRecord))
        {
            throw new InvalidOperationException($"The discriminator for type '{previousVersionType.FullName}' is not registered.");
        }

        ///if (previousVersionRecord.Version >= version)
        ///{
        ///    throw new InvalidOperationException($"The previous version '{previousVersionRecord.Version}' must be less than the passing one '{version}'.");
        ///}

        var versionUpgrader = _serviceProvider.GetRequiredService<TVersionUpgrader>();
        var versionUpgraderProxy = DataVersionUpgraderProxy.Create(versionUpgrader);

        var typeName = typeof(T).FullName.NotNull();
        var record = _discriminatorsByTypeName[typeName];
        record.PreviousVersions.Add(new TypePreviousVersion(previousVersionType, versionUpgraderProxy));
    }

    public bool HasMapping(Type type)
    {
        var typeName = type.FullName.NotNull();
        return _discriminatorsByTypeName.ContainsKey(typeName)
            || _discriminatorsByBaseTypeName.ContainsKey(typeName);
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

    public (TypeDiscriminatorRecord, TypePreviousVersion)? GetDiscriminatorByPreviousVersion(Type type)
    {
        Guard.NotNull(type);

        foreach (var record in _discriminatorsByTypeName.Values)
        {
            var previousVersion = record.PreviousVersions.Find(x => x.PreviousVersionType == type);
            if (previousVersion is not null)
            {
                return (record, previousVersion);
            }
        }

        return null;
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
        _logger.LogTrace("Type '{Type}' mapped as '{Discriminator}' version '{Version}'",
            record.Type.FullName, record.Discriminator, record.Version);

        _discriminatorsByDiscriminatorAndVersion.Add(CreateDiscriminatorQualifiedName(record.Discriminator, record.Version), record);
        _discriminatorsByTypeName.Add(record.Type.FullName.NotNull(), record);

        // Retrieve all the base types
        var baseType = record.Type.BaseType;
        while (baseType is not null)
        {
            if (baseType == typeof(object))
            {
                break;
            }

            RegisterType(baseType);

            baseType = baseType.BaseType;
        }

        // Retrieve interfaces
        var interfaces = record.Type.GetInterfaces().NotNull();
        foreach (var @interface in interfaces)
        {
            if (@interface.Namespace?.StartsWith("System.", StringComparison.Ordinal) == true)
            {
                continue;
            }

            RegisterType(@interface);
        }

        void RegisterType(Type registerType)
        {
            if (registerType.FullName is null)
            {
                return;
            }

            if (!_discriminatorsByBaseTypeName.TryGetValue(registerType.FullName, out var records))
            {
                records = new List<TypeDiscriminatorRecord>();
                _discriminatorsByBaseTypeName.Add(registerType.FullName, records);
            }

            records.Add(record);
        }
    }

    private static string CreateDiscriminatorQualifiedName(string discriminator, int version)
    {
        return $"{discriminator};{version}";
    }
}
