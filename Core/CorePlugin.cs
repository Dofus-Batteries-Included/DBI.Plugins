using BepInEx;
using BepInEx.Unity.IL2CPP;
using DofusBatteriesIncluded.Core.Behaviours;
using Il2CppInterop.Runtime.Injection;

namespace DofusBatteriesIncluded.Core;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class CorePlugin : BasePlugin
{
    public override void Load()
    {
        DBI.Enabled = DBI.Configuration.Bind("General", "Master toggle", true, "Enable or disable all Dofus Batteries Included plugins.");

        if (!DBI.Enabled)
        {
            Log.LogInfo("Dofus Batteries Included is disabled.");
            return;
        }

        AddComponent<DofusBatteriesIncludedCore>();
        AddComponent<DofusBatteriesIncludedCommands>();
        ClassInjector.RegisterTypeInIl2Cpp<DofusBatteriesIncludedWindow>();
        CoreWindow window = AddComponent<CoreWindow>();
        DofusBatteriesIncludedGameMenu menu = AddComponent<DofusBatteriesIncludedGameMenu>();

        menu.AddButton("Dofus Batteries Included", evt => window.Toggle());
    }
}
