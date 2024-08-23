using System;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.TreasureSolver.Behaviours;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class TreasureHuntSolverPlugin : DBIPlugin
{
    protected override Task StartAsync()
    {
        AddComponent<TreasureHuntManager>();

        string directory = Path.GetDirectoryName(typeof(TreasureHuntSolverPlugin).Assembly.Location);
        string basePath = directory == null ? "" : Path.GetFullPath(directory);
        string path = Path.Combine(basePath, "Resources", "dofuspourlesnoobs_clues.json");
        ClueFinders.RegisterFinder("Dofus pour les noobs (offline)", async () => await LoadDplbClueFinder(path));

        DBI.Messaging.RegisterListener<TreasureHuntEventListener>();
        return Task.CompletedTask;
    }

    async Task<StaticClueFinder> LoadDplbClueFinder(string path)
    {
        try
        {
            StaticClueFinder dplbClueFinder = await DofusPourLesNoobsStaticClueFinderFactory.Create(path);
            Log.LogInformation("Successfully loaded Dofus pour les noobs offline clue finder from file {Path}.", path);
            return dplbClueFinder;
        }
        catch (Exception exn)
        {
            Log.LogError(exn, "Unexpected error while loading DPLB clue finder from path {Path}: {Message}.", path, exn.Message);
            return null;
        }
    }
}
