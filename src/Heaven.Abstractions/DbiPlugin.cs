using Heaven.Abstractions.Configuration;
using Microsoft.Extensions.Logging;

namespace Heaven.Abstractions;

public abstract class DbiPlugin
{
    protected DbiPlugin(DbiPluginInfo info, ILogger<DbiPlugin> logger)
    {
        Info = info;
        Logger = logger;
    }

    public DbiPluginInfo Info { get; private set; }
    public ILogger<DbiPlugin> Logger { get; }
    public bool Started { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (Started)
        {
            Logger.LogInformation("Plugin {DisplayName} ({Name}) is already started.", Info.DisplayName, Info.Name);
            return;
        }

        Started = true;
        await OnStartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!Started)
        {
            Logger.LogInformation("Plugin {DisplayName} ({Name}) is not started yet.", Info.DisplayName, Info.Name);
            return;
        }

        Started = false;
        await OnStopAsync(cancellationToken);
    }

    public virtual void SetupConfiguration(IPluginConfigurationBuilder configurationBuilder) { }
    protected virtual Task OnStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    protected virtual Task OnStopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
