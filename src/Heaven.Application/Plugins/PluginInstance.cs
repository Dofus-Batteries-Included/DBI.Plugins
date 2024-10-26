using DBI.Heaven.Application.Configuration;
using DBI.Heaven.Application.Plugins.Notifications;
using DBI.HellHeavenInterop;
using Heaven.Abstractions;
using MediatR;
using PluginConfiguration = DBI.Heaven.Application.Configuration.PluginConfiguration;
using PluginConfigurationCategory = DBI.Heaven.Application.Configuration.PluginConfigurationCategory;
using PluginConfigurationEntry = DBI.Heaven.Application.Configuration.PluginConfigurationEntry;

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
            await SetStatusAsync(new PluginStarting());
            await Plugin.StartAsync(cancellationToken);
            await SetStatusAsync(new PluginRunning());
        }
        catch (Exception exn)
        {
            await SetStatusAsync(new PluginFailedToStart(exn));
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SetStatusAsync(new PluginStopping());
            await Plugin.StopAsync(cancellationToken);
            await SetStatusAsync(new PluginStopped());
        }
        catch (Exception exn)
        {
            await SetStatusAsync(new PluginCrashed(exn));
            throw;
        }
    }

    public async Task UpdateConfigurationAsync(PluginConfigurationValues configuration)
    {
        foreach (PluginConfigurationCategoryValues? category in configuration.Categories)
        {
            PluginConfigurationCategory? pluginCategory = Configuration.GetCategory(category.Name);
            if (pluginCategory == null)
            {
                throw new InvalidOperationException($"Could not find category {category.Name} in configuration of plugin {Plugin}.");
            }

            foreach (PluginConfigurationEntryValue? entry in category.Entries)
            {
                PluginConfigurationEntry? pluginEntry = pluginCategory.GetEntry(entry.Name);
                if (pluginEntry == null)
                {
                    throw new InvalidOperationException($"Could not find entry {category.Name}: {entry.Name} in configuration of plugin {Plugin}.");
                }

                switch (entry.PluginConfigurationEntryValueCase)
                {
                    case PluginConfigurationEntryValue.PluginConfigurationEntryValueOneofCase.BoolValue:
                        ((PluginConfigurationEntry<bool>)pluginEntry).SetValue(entry.BoolValue);
                        break;
                    case PluginConfigurationEntryValue.PluginConfigurationEntryValueOneofCase.StringValue:
                        ((PluginConfigurationEntry<string>)pluginEntry).SetValue(entry.StringValue);
                        break;
                    case PluginConfigurationEntryValue.PluginConfigurationEntryValueOneofCase.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(entry.PluginConfigurationEntryValueCase), entry.PluginConfigurationEntryValueCase, null);
                }
            }
        }

        await mediator.Publish(new PluginConfigurationChangedNotification(this));
    }

    public override string ToString() => $"{Plugin}";

    async Task SetStatusAsync(PluginStatus status)
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
