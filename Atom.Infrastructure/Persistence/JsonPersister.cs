using System;
using System.Text.Json;
using System.Threading;
using Genius.Atom.Infrastructure.Io;

namespace Genius.Atom.Infrastructure.Persistence;

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

    public JsonPersister(IFileService io)
    {
        _io = io;
        _jsonOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public T? Load<T>(string filePath)
    {
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