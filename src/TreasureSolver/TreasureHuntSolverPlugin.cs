using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Metadata;
using DofusBatteriesIncluded.TreasureSolver.Behaviours;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Core.MyPluginInfo.PLUGIN_GUID)]
class TreasureHuntSolverPlugin : DBIPlugin
{
    public TreasureHuntSolverPlugin() : base(GetExpectedBuildIdFromAssemblyAttribute()) { }

    protected override Task StartAsync()
    {
        AddComponent<TreasureHuntManager>();

        ClueFinders.RegisterFinder(ClueFinderConfig.DofusPourLesNoobs.Name, ClueFinderConfig.DofusPourLesNoobs.DisplayName, async () => await LoadDplbClueFinder());
        ClueFinders.RegisterFinder(ClueFinderConfig.DofusMapHunt.Name, ClueFinderConfig.DofusMapHunt.DisplayName, async () => await DofusHuntClueFinder.Create());

        DBI.Configuration.Configure("Treasure Hunt", "Clue Finder", ClueFinderConfig.DofusPourLesNoobs.Name)
            .WithPossibleValues(ClueFinderConfig.DofusPourLesNoobs.Name, ClueFinderConfig.DofusMapHunt.Name)
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

    record struct ClueFinderConfig
    {
        public static ClueFinderConfig DofusPourLesNoobs { get; } = new("DofusPourLesNoobs", "Dofus pour les noobs (offline)");
        public static ClueFinderConfig DofusMapHunt { get; } = new("DofusMapHunt", "Dofus Map Hunt");

        ClueFinderConfig(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public string Name { get; }
        public string DisplayName { get; }

        public override string ToString() => Name;
    }

    static Guid? GetExpectedBuildIdFromAssemblyAttribute() => typeof(TreasureHuntSolverPlugin).Assembly.GetCustomAttribute<ExpectedDofusBuildIdAttribute>()?.BuildId;
}
