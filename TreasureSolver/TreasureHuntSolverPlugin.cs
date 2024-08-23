using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Maps;
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

        ClueFinders.RegisterFinder(ClueFinderConfig.DofusPourLesNoobs.Name, ClueFinderConfig.DofusPourLesNoobs.DisplayName, async () => await LoadDplbClueFinder());
        ClueFinders.RegisterFinder(ClueFinderConfig.DofusHunt.Name, ClueFinderConfig.DofusHunt.DisplayName, async () => await LoadDofusHuntFinder());

        DBI.Configuration.Configure("Treasure Hunt", "Clue Finder", ClueFinderConfig.DofusPourLesNoobs.Name)
            .WithPossibleValues(ClueFinderConfig.DofusPourLesNoobs.Name, ClueFinderConfig.DofusHunt.Name)
            .WithDescription("The source of clues to use when solving treasure hunts.")
            .RegisterChangeCallback(ClueFinders.SetDefaultFinder, true)
            .Bind();

        DBI.Messaging.RegisterListener<TreasureHuntEventListener>();
        return Task.CompletedTask;
    }

    async Task<StaticClueFinder> LoadDplbClueFinder()
    {
        string directory = Path.GetDirectoryName(typeof(TreasureHuntSolverPlugin).Assembly.Location);
        string basePath = directory == null ? "" : Path.GetFullPath(directory);
        string path = Path.Combine(basePath, "Resources", "dofuspourlesnoobs_clues.json");

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

    static Task<IClueFinder> LoadDofusHuntFinder() => Task.FromResult<IClueFinder>(new StaticClueFinder(new Dictionary<Position, IReadOnlyCollection<int>>()));

    record struct ClueFinderConfig : IEquatable<ClueFinderConfig>
    {
        public static ClueFinderConfig DofusPourLesNoobs { get; } = new("DofusPourLesNoobs", "Dofus pour les noobs (offline)");
        public static ClueFinderConfig DofusHunt { get; } = new("DofusHunt", "Dofus hunt");

        ClueFinderConfig(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public string Name { get; }
        public string DisplayName { get; }

        public override string ToString() => Name;
    }
}
