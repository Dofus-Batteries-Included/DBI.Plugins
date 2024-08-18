using BepInEx;
using BepInEx.Unity.IL2CPP;
using DofusBatteriesIncluded.Core.Behaviours;

namespace DofusBatteriesIncluded.Core;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class CorePlugin : BasePlugin
{
    public override void Load() => AddComponent<DofusBatteriesIncludedCore>();
}
