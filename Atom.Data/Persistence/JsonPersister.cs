using System.Text.Json;
using System.Text.Json.Serialization;
using Genius.Atom.Infrastructure.Io;

namespace Genius.Atom.Data.Persistence;

public interface IJsonPersister
{
    T? Load<T>(string filePath);
    T[]? LoadCollection<T>(string filePath);
    void Store(string filePath, object data);
}

internal sealed class JsonPersister : IJsonPersister
{
    private readonly IFileService _io;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly static ReaderWriterLockSlim _locker = new();

    public JsonPersister(IFileService io, ITypeDiscriminators typeDiscriminators, IEnumerable<IJsonConverter> converters)
    {
        _io = io.NotNull();
        _jsonOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Converters = { new DiscriminatedTypeConverterFactory(typeDiscriminators) },
        };

        foreach (var converter in converters)
        {
            if (converter is not JsonConverter jsonConverter)
            {
                throw new NotSupportedException("The provided converter is not a JsonConverter");
            }

            _jsonOptions.Converters.Add(jsonConverter);
        }
    }

    public T? Load<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return default;
        }

        _locker.EnterReadLock();
        try
        {
            if (!_io.FileExists(filePath))
            {
                return default;
            }
            var content = _io.ReadTextFromFile(filePath);
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public T[]? LoadCollection<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return default;
        }

        _locker.EnterReadLock();
        try
        {
            if (!_io.FileExists(filePath))
            {
                return Array.Empty<T>();
            }
            var content = _io.ReadTextFromFile(filePath);
            return JsonSerializer.Deserialize<T[]>(content, _jsonOptions);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public void Store(string filePath, object data)
    {
        if (string.IsNullOrWhiteSpace(filePath)
            || data is null)
        {
            return;
        }

        _locker.EnterWriteLock();
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            _io.WriteTextToFile(filePath, json);
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }
}