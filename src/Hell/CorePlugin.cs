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

    public override void Load()
    {
        ILogger logger = Logging.Create<CorePlugin>();

        logger.LogInformation("Starting Heaven...");
        if (HeavenInteroperability.StartHeaven().GetAwaiter().GetResult())
        {
            logger.LogInformation("Heaven started successfully.");
        }
        else
        {
            logger.LogInformation("Could not start Heaven.");
        }
    }
}
