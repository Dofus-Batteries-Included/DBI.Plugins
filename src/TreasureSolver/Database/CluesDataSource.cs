using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DofusBatteriesIncluded.TreasureSolver.Database;

public class CluesDataSource : IReadOnlyCluesDataSource
{
    readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    readonly Dictionary<long, List<ClueRecord>> _clues = [];

    public IReadOnlyCollection<ClueRecord> GetRecords(long mapId) => _clues.GetValueOrDefault(mapId) ?? [];

    public void AddRecords(long mapId, params ClueRecord[] records)
    {
        if (!_clues.ContainsKey(mapId))
        {
            _clues[mapId] = [];
        }

        _clues[mapId].AddRange(records);
    }
}

public class JsonFileCluesDataSource : IReadOnlyCluesDataSource
{
    static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    readonly Dictionary<long, List<ClueRecord>> _clues;
    readonly string _filePath;

    public JsonFileCluesDataSource(string filePath)
    {
        _filePath = filePath;
        _clues = LoadFrom(filePath);
    }

    public IReadOnlyCollection<ClueRecord> GetRecords(long mapId) => _clues.GetValueOrDefault(mapId) ?? [];

    public void AddRecords(long mapId, params ClueRecord[] records)
    {
        if (!_clues.ContainsKey(mapId))
        {
            _clues[mapId] = [];
        }

        foreach (ClueRecord record in records)
        {
            _clues[mapId].Add(record);
        }

        SaveTo(_filePath, _clues);
    }

    void SaveTo(string filePath, Dictionary<long, List<ClueRecord>> clues)
    {
        using FileStream stream = File.OpenRead(filePath);
        JsonSerializer.Serialize(stream, _clues, JsonSerializerOptions);
    }

    static Dictionary<long, List<ClueRecord>> LoadFrom(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        using FileStream stream = File.OpenRead(filePath);
        return JsonSerializer.Deserialize<Dictionary<long, List<ClueRecord>>>(stream, JsonSerializerOptions);
    }
}
