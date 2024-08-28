using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Maps;
using DofusBatteriesIncluded.TreasureSolver.Clues.Data;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public class LocalCluesService : ICluesService
{
    static readonly ILogger Log = DBI.Logging.Create<LocalCluesService>();

    readonly ICluesDataSource[] _externalSources;
    readonly JsonFileCluesDataSource _localSource;
    readonly List<ICluesDataSource> _dataSources;

    LocalCluesService(JsonFileCluesDataSource localSource, params ICluesDataSource[] externalSources)
    {
        _externalSources = externalSources;
        _localSource = localSource;
        _dataSources = externalSources.ToList();
        _dataSources.Add(_localSource);
    }

    public Task<long?> FindMapOfNextClue(long startMapId, Direction direction, int clueId, int cluesMaxDistance)
    {
        long[] maps = MapUtils.MoveInDirection(startMapId, direction).Take(cluesMaxDistance).ToArray();
        foreach (long map in maps)
        {
            IReadOnlyCollection<int> clues = _dataSources.GetCluesAt(map);
            if (clues.Contains(clueId))
            {
                return Task.FromResult<long?>(map);
            }
        }

        return Task.FromResult<long?>(null);
    }

    public Task RegisterCluesAsync(long mapId, params ClueWithStatus[] clues)
    {
        ClueWithStatus[] found = clues.Where(c => c.IsPresent).ToArray();
        if (found.Length > 0)
        {
            Log.LogInformation("Clue(s) {Clues} were found in map {MapId}...", string.Join(", ", found.Select(f => f.ClueId)), mapId);
        }

        ClueWithStatus[] notFound = clues.Where(c => !c.IsPresent).ToArray();
        if (notFound.Length > 0)
        {
            Log.LogInformation("Clue(s) {Clues} were not found in map {MapId}...", string.Join(", ", notFound.Select(f => f.ClueId)), mapId);
        }

        DateTime now = DateTime.Now;
        _localSource.AddRecords(mapId, clues.Select(x => new ClueRecord { ClueId = x.ClueId, WasFound = x.IsPresent, RecordDate = now }).ToArray());

        return Task.CompletedTask;
    }

    public static LocalCluesService Create()
    {
        Log.LogInformation("Starting LOCAL clues service...");

        string directory = Path.GetDirectoryName(typeof(TreasureHuntSolverPlugin).Assembly.Location);
        string basePath = directory == null ? "" : Path.GetFullPath(directory);
        string path = Path.Combine(basePath, "Resources", "dofuspourlesnoobs_clues.json");

        List<ICluesDataSource> clues = [];
        if (File.Exists(path))
        {
            Log.LogInformation("Initializing clues from DPLB file at {Path}...", path);
            ICluesDataSource dplbSource = DofusPourLesNoobsCluesLoader.LoadClues(path);
            clues.Add(dplbSource);
            Log.LogInformation("Loaded a total of {CluesCount} clues in {MapsCout} maps.", dplbSource.CluesCount, dplbSource.MapsCount);
        }
        else
        {
            Log.LogError("Could not find DPLB file at {Path}, will not load clues.", path);
        }

        string localSourcePath = Path.Join(DBI.AppDataFolder, "TreasureSolver", "clues.json");
        Log.LogInformation("Initializing local source of clues from file at {Path}...", localSourcePath);
        JsonFileCluesDataSource localSource = new(localSourcePath);
        Log.LogInformation("Loaded a total of {CluesCount} clues in {MapsCout} maps.", localSource.CluesCount, localSource.MapsCount);

        Log.LogInformation("LOCAL clues service ready.");

        return new LocalCluesService(localSource, clues.ToArray());
    }
}

public record struct ClueWithStatus(int ClueId, bool IsPresent);
