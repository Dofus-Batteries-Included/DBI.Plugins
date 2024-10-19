using DBI.Heaven.Application.Configuration;
using Heaven.Abstractions;

namespace DBI.Heaven.Application.Plugins;

class PluginInstance(DbiPlugin plugin, PluginConfiguration configuration)
{
    public DbiPlugin Plugin { get; } = plugin;
    public PluginConfiguration Configuration { get; } = configuration;
    public bool Started { get; internal set; }
    public bool FailedToStart { get; internal set; }
    public string? FailedToStartReason { get; internal set; }
}
