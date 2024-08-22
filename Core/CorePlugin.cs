using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Core.Behaviours;
using DofusBatteriesIncluded.Core.Protocol;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace DofusBatteriesIncluded.Core;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class CorePlugin : DBIPlugin
{
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

        return Task.CompletedTask;
    }
}
