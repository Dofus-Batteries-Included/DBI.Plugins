using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Core.DataCenter;
using Core.DataCenter.Metadata.Quest.TreasureHunt;
using Core.DataCenter.Metadata.World;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Extensions;
using DofusBatteriesIncluded.TreasureSolver.Clues.Data;
using Microsoft.Extensions.Logging;
using MapCoordinates = Core.DataCenter.Metadata.World.MapCoordinates;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public static class DofusPourLesNoobsCluesLoader
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(DofusPourLesNoobsCluesLoader));

    public static ICluesDataSource LoadClues(string dplbFilePath)
    {
        List<(int Id, string Name)> gamePois = GetGamePois();

        DPLBFile parsedFile;
        using (FileStream stream = File.OpenRead(dplbFilePath))
        {
            parsedFile = JsonSerializer.Deserialize<DPLBFile>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        DateTime fileDate = File.GetLastWriteTime(dplbFilePath);

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
        CluesDataSource result = new();
        MapCoordinatesRoot coordinates = DataCenterModule.GetDataRoot<MapCoordinatesRoot>();
        MapPositionsRoot positions = DataCenterModule.GetDataRoot<MapPositionsRoot>();
        foreach (MapClues mapClues in parsedFile.Maps)
        {
            MapCoordinates mapCoordinates = coordinates.GetMapCoordinatesByCoords(mapClues.X, mapClues.Y);
            int[] clueIds = mapClues.Clues.Where(c => dplbClueToGameClueMapping.ContainsKey(c)).Select(c => dplbClueToGameClueMapping[c]).ToArray();

            if (mapCoordinates.mapIds != null)
            {
                for (int i = 0; i < mapCoordinates.mapIds.Count; i++)
                {
                    MapPositions position = positions.GetMapPositionById(mapCoordinates.mapIds[i]);
                    if (position is not { worldMap: 1 })
                    {
                        continue;
                    }

                    result.AddRecords(position.id, clueIds.Select(clueId => new ClueRecord { ClueId = clueId, WasFound = true, RecordDate = fileDate }).ToArray());

                    cluesCount += clueIds.Length;
                    mapsCount++;
                }
            }
        }

        return result;
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
