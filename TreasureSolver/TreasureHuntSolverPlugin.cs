using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.TreasureSolver.Behaviours;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using DofusBatteriesIncluded.TreasureSolver.Clues.Serialization;
using DofusBatteriesIncluded.TreasureSolver.Models;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class TreasureHuntSolverPlugin : DBIPlugin
{
    protected override async Task StartAsync()
    {
        string directory = Path.GetDirectoryName(typeof(TreasureHuntSolverPlugin).Assembly.Location);
        string basePath = directory == null ? "" : Path.GetFullPath(directory);

        await LoadStaticSolverFromJsonFile("Dofus pour les noobs (offline)", Path.Combine(basePath, "Resources", "dofuspourlesnoobs_clues.json"));

        AddComponent<TreasureHuntWindowAccessor>();
    }

    async Task LoadStaticSolverFromJsonFile(string name, string filePath)
    {
        try
        {
            await using FileStream stream = File.OpenRead(filePath);
            JsonCluesFileParser parser = new();
            Dictionary<Position, int[]> clues = await parser.ParseAsync(stream);
            StaticClueFinder finder = new(clues);
            ClueFinders.RegisterFinder(name, finder);
            Log.LogInformation("Successfully loaded static clue finder {Name} from file {Path}.", name, filePath);
        }
        catch (Exception exn)
        {
            Log.LogError(exn, "Error while loading static clue finder {Name} from file {Path}: {Message}.", name, filePath, exn.Message);
        }
    }
}
