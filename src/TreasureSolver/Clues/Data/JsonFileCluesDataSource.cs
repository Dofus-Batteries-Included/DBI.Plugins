using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Path = Il2CppSystem.IO.Path;

namespace DofusBatteriesIncluded.TreasureSolver.Clues.Data;

public class JsonFileCluesDataSource : ICluesDataSource
{
    static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    readonly CluesDataSource _cache;
    readonly string _filePath;

    public JsonFileCluesDataSource(string filePath)
    {
        _filePath = filePath;
        _cache = new CluesDataSource(LoadFrom(filePath));
    }

    public int MapsCount => _cache.MapsCount;
    public int CluesCount => _cache.CluesCount;
    public IReadOnlyDictionary<long, IReadOnlyCollection<ClueRecord>> Clues => _cache.Clues;

    public IReadOnlyCollection<ClueRecord> GetRecords(long mapId) => _cache.GetRecords(mapId);

    public void AddRecords(long mapId, params ClueRecord[] records)
    {
        _cache.AddRecords(mapId, records);
        SaveTo(_filePath, _cache.Clues);
    }

    static void SaveTo(string filePath, IReadOnlyDictionary<long, IReadOnlyCollection<ClueRecord>> clues)
    {
        string dirName = Path.GetDirectoryName(filePath);
        if (dirName is not null && !Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName);
        }

        using FileStream stream = File.Open(filePath, FileMode.Create);
        JsonSerializer.Serialize(stream, clues, JsonSerializerOptions);
    }

    static IReadOnlyDictionary<long, IReadOnlyCollection<ClueRecord>> LoadFrom(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new Dictionary<long, IReadOnlyCollection<ClueRecord>>();
        }

        using FileStream stream = File.OpenRead(filePath);
        return JsonSerializer.Deserialize<Dictionary<long, List<ClueRecord>>>(stream, JsonSerializerOptions)
            .ToDictionary(kv => kv.Key, IReadOnlyCollection<ClueRecord> (kv) => kv.Value);
    }
}
