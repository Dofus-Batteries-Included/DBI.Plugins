using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Maps;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver.Database;

public class LocalCluesService : ICluesService
{
    static readonly ILogger Log = DBI.Logging.Create<LocalCluesService>();

    readonly Dictionary<long, HashSet<int>> _clues;

    public LocalCluesService(IReadOnlyDictionary<long, IReadOnlyCollection<int>> clues)
    {
        _clues = clues.ToDictionary(kv => kv.Key, kv => kv.Value.ToHashSet());
    }

    public Task<long?> FindMapOfNextClue(long startMapId, Direction direction, int clueId, int cluesMaxDistance)
    {
        long[] maps = MapUtils.MoveInDirection(startMapId, direction).Take(cluesMaxDistance).ToArray();
        foreach (long map in maps)
        {
            if (!_clues.TryGetValue(map, out HashSet<int> clues))
            {
                continue;
            }

            if (clues.Contains(clueId))
            {
                return Task.FromResult<long?>(map);
            }
        }

        return Task.FromResult<long?>(null);
    }

    public Task RegisterCluesAsync(long mapId, params int[] clues)
    {
        if (!_clues.ContainsKey(mapId))
        {
            _clues[mapId] = [];
        }

        foreach (int clue in clues)
        {
            _clues[mapId].Add(clue);
        }

        return Task.CompletedTask;
    }

    public static LocalCluesService Create()
    {
        Log.LogInformation("Starting LOCAL clues service...");

        string directory = Path.GetDirectoryName(typeof(TreasureHuntSolverPlugin).Assembly.Location);
        string basePath = directory == null ? "" : Path.GetFullPath(directory);
        string path = Path.Combine(basePath, "Resources", "dofuspourlesnoobs_clues.json");

        IReadOnlyDictionary<long, IReadOnlyCollection<int>> clues;
        if (File.Exists(path))
        {
            Log.LogInformation("Initializing clues from DPLB file at {Path}...", path);
            clues = DofusPourLesNoobsCluesLoader.LoadClues(path);
        }
        else
        {
            Log.LogError("Could not find DPLB file at {Path}, will not load clues.", path);
            clues = new Dictionary<long, IReadOnlyCollection<int>>();
        }

        Log.LogInformation("LOCAL clues service ready.");

        return new LocalCluesService(clues);
    }
}
