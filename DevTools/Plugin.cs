using BepInEx;
using BepInEx.Unity.IL2CPP;
using DofusBatteriesIncluded.DevTools.Behaviours;

namespace DofusBatteriesIncluded.DevTools;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
public class Plugin : BasePlugin
{
    public override void Load() => AddComponent<EnableQuantumConsole>();
}
