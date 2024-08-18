using BepInEx;
using BepInEx.Unity.IL2CPP;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.DevTools.Behaviours;
using Il2CppSystem;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.DevTools;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
[BepInDependency(Core.MyPluginInfo.PLUGIN_GUID)]
class DevToolsPlugin : BasePlugin
{
    public override void Load()
    {
        if (!DBI.Enabled)
        {
            Log.LogInfo("Dofus Batteries Included is disabled.");
            return;
        }

        AddComponent<EnableQuantumConsole>();
        AddComponent<LogSceneLoaded>();

        DBI.Commands.Register("dump-all-gameobjects", KeyCode.F9, (Action)DumpAllGameObjects);
    }

    static void DumpAllGameObjects()
    {
        ILogger log = DBI.Logging.Create<DevToolsPlugin>();
        foreach (GameObject go in Helpers.FindObjectsOfType<GameObject>())
        {
            log.LogInformation("GameObject {Name} (active: {Active})", go.name, go.activeSelf);
            foreach (MonoBehaviour behaviour in go.GetComponents<MonoBehaviour>())
            {
                log.LogInformation("\t- {DisabledOrNull}Behaviour {Name}", behaviour.enabled ? "" : "DISABLED ", behaviour.GetIl2CppType().Name);
            }
        }
    }
}
