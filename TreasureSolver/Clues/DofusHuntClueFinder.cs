using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.DataCenter;
using Core.DataCenter.Metadata.Quest.TreasureHunt;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Extensions;
using DofusBatteriesIncluded.Core.Maps;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public class DofusHuntClueFinder : IClueFinder
{
    static readonly ILogger Log = DBI.Logging.Create<DofusHuntClueFinder>();

    readonly Dictionary<int, int> _gameCluesToDofusMapHuntClues;

    DofusHuntClueFinder(Dictionary<int, int> gameCluesToDofusMapHuntClues)
    {
        _gameCluesToDofusMapHuntClues = gameCluesToDofusMapHuntClues;
    }

    public async Task<Position?> FindPositionOfNextClue(Position start, Direction direction, int clueId, int maxDistance)
    {
        if (!_gameCluesToDofusMapHuntClues.TryGetValue(clueId, out int dmhClueId))
        {
            return null;
        }

        string response;
        try
        {
            response = await GetAsync($"https://dofus-map.com/huntTool/getData.php?x={start.X}&y={start.Y}&direction={direction}&world=0&language=fr");
        }
        catch (Exception exn)
        {
            Log.LogError(exn, "Error while fetching result from Dofus Map Hunt.");
            return null;
        }

        DofusMapHuntApiResponse parsedResponse;
        try
        {
            parsedResponse = JsonSerializer.Deserialize<DofusMapHuntApiResponse>(response);
        }
        catch (Exception exn)
        {
            Log.LogError(exn, "Error while parsing response of Dofus Map Hunt.");
            return null;
        }

        Clue hint = parsedResponse.Hints.OrderBy(h => h.D).FirstOrDefault(h => h.N == dmhClueId);
        if (hint == null)
        {
            return null;
        }

        return new Position(hint.X, hint.Y);
    }

    static async Task<string> GetAsync(string uri)
    {
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(uri);
        string content = await response.Content.ReadAsStringAsync();
        return content;
    }

    class DofusMapHuntApiResponse
    {
        public IReadOnlyList<Clue> Hints { get; init; }
    }

    class Clue
    {
        public int N { get; init; }
        public int X { get; init; }
        public int Y { get; init; }
        public int D { get; init; }
    }

    public static async Task<DofusHuntClueFinder> Create()
    {
        // IMPORTANT: do this before the first await, it MUST be performed in the main thread.
        List<(int Id, string Name)> gamePois = GetGamePois();

        Dictionary<int, int> mapping = [];

        string page = await GetAsync("https://dofus-map.com/fr/hunt");
        Regex regex = new(@"var text = (?<json>\{.*\})");
        Match match = regex.Match(page);
        if (!match.Success)
        {
            Log.LogError("Could not find clue names in Dofus Map Hunt page.");
            return null;
        }

        Dictionary<string, string> clueNames;
        try
        {
            clueNames = JsonSerializer.Deserialize<Dictionary<string, string>>(match.Groups["json"].Value);
        }
        catch (Exception exn)
        {
            Log.LogError(exn, "Could not parse clue names from Dofus Map Hunt page.");
            return null;
        }

        foreach ((int id, string name) in gamePois)
        {
            string nameWithoutAccent = name.RemoveAccents();
            KeyValuePair<string, string> dofusMapHuntClue = clueNames.FirstOrDefault(c => c.Value.RemoveAccents() == nameWithoutAccent);
            if (!int.TryParse(dofusMapHuntClue.Key, out int dofusMapHuntClueId))
            {
                Log.LogWarning("Could not find clue {Name} ({Id}) in Dofus Map Hunt clues.", nameWithoutAccent, id);
                continue;
            }

            mapping[id] = dofusMapHuntClueId;
        }

        return new DofusHuntClueFinder(mapping);
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
}
