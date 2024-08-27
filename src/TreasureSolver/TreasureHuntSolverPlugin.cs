using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.TreasureSolver.Behaviours;

namespace DofusBatteriesIncluded.TreasureSolver;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Core.MyPluginInfo.PLUGIN_GUID)]
class TreasureHuntSolverPlugin : DBIPlugin
{
    protected override async Task StartAsync()
    {
        AddComponent<TreasureHuntManager>();
        DBI.Messaging.RegisterListener<TreasureHuntEventListener>();
    }
}
