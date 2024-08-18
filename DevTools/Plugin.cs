using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DofusBatteriesIncluded.DevTools.Behaviours;

namespace DofusBatteriesIncluded.DevTools;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
public class Plugin : BasePlugin
{
    public static new ManualLogSource Log { get; private set; }

    public override void Load()
    {
        Log = base.Log;
        AddComponent<EnableQuantumConsole>();
        AddComponent<LogSceneChange>();
    }
}
