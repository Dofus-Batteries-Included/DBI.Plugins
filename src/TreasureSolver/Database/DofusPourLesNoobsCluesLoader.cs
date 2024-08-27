using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Core.DataCenter;
using Core.DataCenter.Metadata.Quest.TreasureHunt;
using Core.DataCenter.Metadata.World;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Extensions;
using Microsoft.Extensions.Logging;
using Xtensive.Orm;
using MapCoordinates = Core.DataCenter.Metadata.World.MapCoordinates;

namespace DofusBatteriesIncluded.TreasureSolver.Database;

public static class DofusPourLesNoobsCluesLoader
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(DofusPourLesNoobsCluesLoader));

    public static async Task LoadCluesAsync(Domain domain, string dplbFilePath)
    {
        await using Session session = await domain.OpenSessionAsync();
        await using TransactionScope transaction = await session.OpenTransactionAsync();

        // IMPORTANT: do this before the first await, it MUST be performed in the main thread.
        List<(int Id, string Name)> gamePois = GetGamePois();

        DPLBFile parsedFile;
        await using (FileStream stream = File.OpenRead(dplbFilePath))
        {
            parsedFile = await JsonSerializer.DeserializeAsync<DPLBFile>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        Dictionary<int, int> dplbClueToGameClueMapping = new();
        foreach ((int id, string name) in gamePois)
        {
            string nameWithoutAccent = name.RemoveAccents();
            ClueNames dplbClue = parsedFile.Clues.FirstOrDefault(
                c => nameWithoutAccent == c.HintFr.RemoveAccents() || nameWithoutAccent == c.HintEn.RemoveAccents() || nameWithoutAccent == c.HintEs.RemoveAccents()
            );
            if (dplbClue is null)
            {
                Log.LogWarning("Could not find clue {Name} ({Id}) in DPLB file.", nameWithoutAccent, id);
                continue;
            }

            dplbClueToGameClueMapping[dplbClue.ClueId] = id;
        }

        int cluesCount = 0;
        int mapsCount = 0;
        MapCoordinatesRoot coordinates = DataCenterModule.GetDataRoot<MapCoordinatesRoot>();
        foreach (MapClues mapClues in parsedFile.Maps)
        {
            MapCoordinates mapCoordinates = coordinates.GetMapCoordinatesByCoords(mapClues.X, mapClues.Y);
            MapPositions[] maps = mapCoordinates.maps._items.Where(m => m is { worldMap: 1 }).ToArray();
            if (maps.Length == 0)
            {
                Log.LogWarning("Could not find map in world 1 at position [{X},{Y}].", mapClues.X, mapClues.Y);
                continue;
            }

            HashSet<int> clueIds = mapClues.Clues.Where(c => dplbClueToGameClueMapping.ContainsKey(c)).Select(c => dplbClueToGameClueMapping[c]).ToHashSet();
            foreach (MapPositions map in maps)
            {
                foreach (int clueId in clueIds)
                {
                    _ = new Clue(map.id, clueId);
                    cluesCount++;
                }
                mapsCount++;
            }
        }

        Log.LogInformation("Loaded a total of {CluesCount} clues in {MapsCout} maps.", cluesCount, mapsCount);

        transaction.Complete();
    }

    static List<(int Id, string Name)> GetGamePois()
    {
        List<(int Id, string Name)> gamePois = [];
        foreach (PointOfInterest poi in DataCenterModule.GetDataRoot<PointOfInterestRoot>().GetObjects())
        {
            gamePois.Add((poi.id, poi.name));
        }
        return gamePois;
    }

    // ReSharper disable once InconsistentNaming
    class DPLBFile
    {
        public ClueNames[] Clues { get; set; }
        public MapClues[] Maps { get; set; }
    }

    class ClueNames
    {
        public int ClueId { get; set; }
        public string HintFr { get; set; }
        public string HintEn { get; set; }
        public string HintEs { get; set; }
    }

    class MapClues
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int[] Clues { get; set; }
    }
}
