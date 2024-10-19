using DBI.Heaven.Application.Configuration;
using Heaven.Abstractions;

namespace DBI.Heaven.Application.Plugins;

class PluginInstance(DbiPlugin plugin, RuntimeConfiguration configuration)
{
    public DbiPlugin Plugin { get; } = plugin;
    public RuntimeConfiguration Configuration { get; } = configuration;
}
