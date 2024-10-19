using BepInEx;
using DBI.Hell.Logging;
using Microsoft.Extensions.Logging;

namespace DBI.Hell;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class CorePlugin
{
    public static readonly CoreLogging Logging = new();

    protected void Load()
    {
        ILogger logger = Logging.Create<CorePlugin>();

        logger.LogInformation("Hello there!");
    }
}
