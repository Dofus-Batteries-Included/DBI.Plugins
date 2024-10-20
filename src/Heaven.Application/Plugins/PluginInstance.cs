using DBI.Heaven.Application.Configuration;
using DBI.Heaven.Application.Plugins.Notifications;
using Heaven.Abstractions;
using MediatR;

namespace DBI.Heaven.Application.Plugins;

class PluginInstance(DbiPlugin plugin, PluginConfiguration configuration, IMediator mediator)
{
    public DbiPlugin Plugin { get; } = plugin;
    public PluginConfiguration Configuration { get; } = configuration;
    public PluginStatus Status { get; private set; } = new PluginNotStarted();

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SetStatus(new PluginStarting());
            await Plugin.StartAsync(cancellationToken);
            await SetStatus(new PluginRunning());
        }
        catch (Exception exn)
        {
            await SetStatus(new PluginFailedToStart(exn));
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SetStatus(new PluginStopping());
            await Plugin.StopAsync(cancellationToken);
            await SetStatus(new PluginStopped());
        }
        catch (Exception exn)
        {
            await SetStatus(new PluginCrashed(exn));
            throw;
        }
    }

    public override string ToString() => $"{Plugin}";

    async Task SetStatus(PluginStatus status)
    {
        PluginStatus oldStatus = Status;
        Status = status;

        await mediator.Publish(new PluginStatusChangedNotification(this, oldStatus, status));
    }

    public record PluginStatus(string Message);

    public record PluginNotStarted() : PluginStatus("Not started");

    public record PluginStarting() : PluginStatus("Starting");

    public record PluginRunning() : PluginStatus("Running");

    public record PluginFailedToStart(Exception Exception) : PluginStatus($"Failed to start: {Exception.Message}");

    public record PluginCrashed(Exception Exception) : PluginStatus($"Crashed: {Exception.Message}");

    public record PluginStopping() : PluginStatus("Stopping");

    public record PluginStopped() : PluginStatus("Stopped");
}
