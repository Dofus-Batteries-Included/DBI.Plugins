using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Core.Behaviours;
using DofusBatteriesIncluded.Core.Metadata;
using DofusBatteriesIncluded.Core.Player;
using DofusBatteriesIncluded.Core.Protocol;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Guid = System.Guid;

namespace DofusBatteriesIncluded.Core;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
class CorePlugin : DBIPlugin
{
    public CorePlugin() : base(GetExpectedBuildIdFromAssemblyAttribute()) { }

    public override bool CanBeDisabled => false;

    protected override Task StartAsync()
    {
        AddComponent<DofusBatteriesIncludedCore>();
        AddComponent<DofusBatteriesIncludedCommands>();
        ClassInjector.RegisterTypeInIl2Cpp<DofusBatteriesIncludedWindow>();
        CoreWindow window = AddComponent<CoreWindow>();
        DofusBatteriesIncludedGameMenu menu = AddComponent<DofusBatteriesIncludedGameMenu>();

        menu.AddButton("Dofus Batteries Included", evt => window.Toggle());

        Harmony.CreateAndPatchAll(typeof(Messaging));

        DBI.Messaging.RegisterListener<UpdateCurrentPlayer>();
        DBI.Messaging.RegisterListener<UpdateCurrentPlayerMap>();

        return Task.CompletedTask;
    }

    static Guid? GetExpectedBuildIdFromAssemblyAttribute() => typeof(CorePlugin).Assembly.GetCustomAttribute<ExpectedDofusBuildIdAttribute>()?.BuildId;
}
