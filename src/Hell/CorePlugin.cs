using BepInEx;
using BepInEx.Unity.IL2CPP;
using DBI.Hell.HeavenInterop;
using DBI.Hell.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DBI.Hell;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class CorePlugin : BasePlugin
{
    public static readonly CoreLogging Logging = new();

    static HeavenHandle? _heavenHandle;
    public static HeavenHandle Handle => _heavenHandle ?? throw new InvalidOperationException("Heaven not started yet.");

    public override void Load()
    {
        ILogger logger = Logging.Create<CorePlugin>();

        logger.LogInformation("Starting heaven...");
        _heavenHandle = HeavenInteroperability.StartHeaven();
        logger.LogInformation("Heaven started successfully.");
    }
}
