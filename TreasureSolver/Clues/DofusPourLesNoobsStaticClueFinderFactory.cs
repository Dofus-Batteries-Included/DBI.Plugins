using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Core.DataCenter;
using Core.DataCenter.Metadata.Quest.TreasureHunt;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Extensions;
using DofusBatteriesIncluded.Core.Maps;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public static class DofusPourLesNoobsStaticClueFinderFactory
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(DofusPourLesNoobsStaticClueFinderFactory));

    public static async Task<StaticClueFinder> Create(string dplbFilePath)
    {
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

        Dictionary<Position, IReadOnlyCollection<int>> clues = parsedFile.Maps.ToDictionary(
            m => new Position(m.X, m.Y),
            IReadOnlyCollection<int> (m) => m.Clues.Where(c => dplbClueToGameClueMapping.ContainsKey(c)).Select(c => dplbClueToGameClueMapping[c]).ToHashSet()
        );

        return new StaticClueFinder(clues);
    }

    static List<(int Id, string Name)> GetGamePois()
    {
        List<(int Id, string Name)> gamePois = [];
        foreach (PointOfInterest poi in DataCenterModule.pointOfInterestRoot.GetObjects())
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
