using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Maps;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using File = System.IO.File;

namespace DofusBatteriesIncluded.TreasureSolver.Database;

public class DatabaseCluesService : ICluesService
{
    static readonly ILogger Log = DBI.Logging.Create<DatabaseCluesService>();
    readonly Domain _domain;

    public DatabaseCluesService(Domain domain)
    {
        _domain = domain;
    }

    public async Task<long?> FindMapOfNextClue(long startMapId, Direction direction, int poiId, int cluesMaxDistance)
    {
        long[] maps = MapUtils.MoveInDirection(startMapId, direction).Take(cluesMaxDistance).ToArray();
        Clue[] clues = await Query.All<Clue>().Where(c => c.ClueId == poiId && maps.Contains(c.MapId)).ToArrayAsync();
        foreach (long map in maps)
        {
            if (clues.Any(c => c.MapId == map))
            {
                return map;
            }
        }

        return null;
    }

    public async Task RegisterCluesAsync(long mapId, params int[] clues)
    {
        await using Session session = await _domain.OpenSessionAsync();
        await using TransactionScope transaction = await session.OpenTransactionAsync();

        int[] existingClues = await session.Query.All<Clue>().Where(c => c.MapId == mapId && clues.Contains(c.ClueId)).Select(c => c.ClueId).ToArrayAsync();
        foreach (int clueId in clues.Except(existingClues))
        {
            _ = new Clue(mapId, clueId);
        }

        transaction.Complete();
    }

    public static async Task<DatabaseCluesService> CreateAsync(string dbPath = ":memory:")
    {
        bool shouldInitialize = !File.Exists(dbPath);

        DomainConfiguration configuration = new($"sqlite:///{dbPath}") { UpgradeMode = DomainUpgradeMode.PerformSafely };
        configuration.Types.Register(typeof(DatabaseCluesService).Assembly);
        Domain domain = await Domain.BuildAsync(configuration);

        if (shouldInitialize)
        {
            string directory = Path.GetDirectoryName(typeof(TreasureHuntSolverPlugin).Assembly.Location);
            string basePath = directory == null ? "" : Path.GetFullPath(directory);
            string path = Path.Combine(basePath, "Resources", "dofuspourlesnoobs_clues.json");

            Log.LogInformation("Initializing clues from DPLB file at {Path}...", path);
            await DofusPourLesNoobsCluesLoader.LoadCluesAsync(domain, path);
        }

        return new DatabaseCluesService(domain);
    }
}
