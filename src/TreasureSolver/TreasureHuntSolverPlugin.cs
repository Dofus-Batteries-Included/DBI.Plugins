using System;
using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Plugins.Core;
using DofusBatteriesIncluded.Plugins.TreasureSolver.Behaviours;
using DofusBatteriesIncluded.Plugins.TreasureSolver.Clues.Listeners;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Core.MyPluginInfo.PLUGIN_GUID)]
class TreasureHuntSolverPlugin : DBIPlugin
{
    protected override Task StartAsync()
    {
        DBI.Configuration.Configure("Treasure Hunt", "Solver", "Remote")
            .WithDescription("Use remote solver shared by all the players or local one.")
            .WithPossibleValues("Remote", "Local")
            .RegisterChangeCallback(
                solver =>
                {
                    TreasureSolver.Solver solverEnum = solver switch
                    {
                        "Remote" => TreasureSolver.Solver.Remote,
                        "Local" => TreasureSolver.Solver.Local,
                        _ => throw new ArgumentOutOfRangeException(nameof(solver), solver, null)
                    };

                    TreasureSolver.SetSolver(solverEnum);
                },
                true
            )
            .Bind();

        AddComponent<TreasureHuntManager>();
        DBI.Messaging.RegisterListener<SaveCluesOnDigAnswerEvent>();

        return Task.CompletedTask;
    }
}
