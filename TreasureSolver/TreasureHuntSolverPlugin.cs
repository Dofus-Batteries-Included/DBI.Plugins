using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using Com.Ankama.Dofus.Server.Game.Protocol.Treasure.Hunt;
using Core.DataCenter;
using Core.DataCenter.Metadata.Quest.TreasureHunt;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Protocol;
using DofusBatteriesIncluded.TreasureSolver.Behaviours;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class TreasureHuntSolverPlugin : DBIPlugin
{
    protected override async Task StartAsync()
    {
        AddComponent<TreasureHuntWindowAccessor>();

        string directory = Path.GetDirectoryName(typeof(TreasureHuntSolverPlugin).Assembly.Location);
        string basePath = directory == null ? "" : Path.GetFullPath(directory);
        string path = Path.Combine(basePath, "Resources", "dofuspourlesnoobs_clues.json");
        ClueFinders.RegisterFinder("Dofus pour les noobs (offline)", async () => await LoadDplbClueFinder(path));

        MessageListener<TreasureHuntEvent> listener = DBI.Messaging.GetListener<TreasureHuntEvent>();
        listener.MessageReceived += (_, message) => Log.LogInformation(
            "Treasure hunt: checkpoint {Checkpoint}/{TotalCheckpoints}, step {Step}/{TotalSteps}",
            message.CurrentCheckPoint,
            message.TotalCheckPoint,
            message.KnownSteps.Count,
            message.TotalStepCount
        );
    }

    async Task<DofusPourLesNoobsStaticClueFinder> LoadDplbClueFinder(string path)
    {
        try
        {
            List<PointOfInterest> pois = [];
            foreach (PointOfInterest poi in DataCenterModule.pointOfInterestRoot.GetObjects())
            {
                pois.Add(poi);
            }

            DofusPourLesNoobsStaticClueFinder dplbClueFinder = await DofusPourLesNoobsStaticClueFinder.Create(path, pois);
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
