using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Path = Il2CppSystem.IO.Path;

namespace DofusBatteriesIncluded.Core.Stores;

public class JsonFileLocalStorage : ILocalStorage
{
    static readonly ILogger Log = DBI.Logging.Create<JsonFileLocalStorage>();

    readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false, PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    readonly string _filePath;
    Dictionary<string, string> _values;

    public JsonFileLocalStorage(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        await LoadIfNecessaryAsync(cancellationToken);
        return _values.GetValueOrDefault(key);
    }

    public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await LoadIfNecessaryAsync(cancellationToken);
        _values[key] = value;
        await SaveAsync(_values, cancellationToken);
    }

    async Task LoadIfNecessaryAsync(CancellationToken cancellationToken = default) => _values ??= await ReadAsync(cancellationToken);

    async Task<Dictionary<string, string>> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new Dictionary<string, string>();
        }

        Dictionary<string, string> result;
        await using (FileStream stream = File.OpenRead(_filePath))
        await using (DeflateStream decompressed = new(stream, CompressionMode.Decompress))
        {
            result = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(decompressed, _jsonOptions, cancellationToken);
        }

        if (result != null)
        {
            return result;
        }

        string name = FindBackupName(_filePath);
        Log.LogWarning("Could not read content of store at {Path}. The file will be backed up at {BackupPath} and replaced.", _filePath, name);

        await using FileStream source = File.OpenRead(_filePath);
        await using FileStream destination = File.OpenWrite(name);
        await source.CopyToAsync(destination, cancellationToken);
        return new Dictionary<string, string>();
    }

    async Task SaveAsync(Dictionary<string, string> values, CancellationToken cancellationToken)
    {
        string directory = Path.GetDirectoryName(_filePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream stream = File.OpenWrite(_filePath);
        await using DeflateStream compressed = new(stream, CompressionMode.Compress);
        await JsonSerializer.SerializeAsync(compressed, values, _jsonOptions, cancellationToken);
    }

    static string FindBackupName(string filePath)
    {
        string name = filePath + ".bak";
        if (!File.Exists(name))
        {
            return name;
        }

        int i = 0;
        while (true)
        {
            string otherName = $"{name}{i}";
            if (!File.Exists(otherName))
            {
                return otherName;
            }

            i++;
        }
    }
}
