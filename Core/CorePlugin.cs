using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using DofusBatteriesIncluded.Core.Behaviours;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;

namespace DofusBatteriesIncluded.Core;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class CorePlugin : BasePlugin
{
    ConfigFile _configFile;
    ConfigEntry<bool> _enabled;

    public override void Load()
    {
        BindConfiguration();

        DBI.Enabled = _enabled.Value;

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

    void BindConfiguration()
    {
        _configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "DofusBatteriesIncluded.cfg"), true);
        _enabled = _configFile.Bind("General", "Enabled", true, "Enable or disable all Dofus Batteries Included plugins.");
    }
}
