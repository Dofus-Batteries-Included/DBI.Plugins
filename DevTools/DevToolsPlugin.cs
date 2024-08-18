using BepInEx;
using BepInEx.Unity.IL2CPP;
using DofusBatteriesIncluded.DevTools.Behaviours;

namespace DofusBatteriesIncluded.DevTools;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
[BepInDependency(Core.MyPluginInfo.PLUGIN_GUID)]
class DevToolsPlugin : BasePlugin
{
    public override void Load()
    {
        if (!Core.DofusBatteriesIncluded.Enabled)
        {
            Log.LogInfo("Dofus Batteries Included is disabled.");
            return;
        }

        AddComponent<EnableQuantumConsole>();
        AddComponent<LogSceneLoaded>();
    }
}
