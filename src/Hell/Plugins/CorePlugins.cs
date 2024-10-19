using DBI.Hell.HeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace DBI.Hell.Plugins;

public class CorePlugins
{
    static readonly ILogger Logger = CorePlugin.Logging.Create<CorePlugins>();
    readonly Dictionary<string, Plugin> _plugins = [];
    public IReadOnlyDictionary<string, Plugin> Plugin => _plugins;

    public async Task LoadFromHeavenAsync()
    {
        HeavenInterop.Plugins.PluginsClient client = new(CorePlugin.Heaven.Channel);
        GetPluginsResponse plugins = await client.GetPluginsAsync(new Empty());

        _plugins.Clear();

        foreach (Plugin plugin in plugins.Plugins)
        {
            _plugins[plugin.Info.Name] = plugin;
            Logger.LogInformation("Found plugin {DisplayName} ({Name}).", plugin.Info.DisplayName, plugin.Info.Name);
        }
    }
}
