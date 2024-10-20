using DBI.Heaven.Application.HellHeavenInterop.Services;
using DBI.Heaven.Application.Plugins.Notifications;
using DBI.HellHeavenInterop;
using MediatR;

namespace DBI.Heaven.Application.HellHeavenInterop.StreamWriters;

class WriteConfigurationChangeOnPluginConfigurationChanged(PluginsHellService pluginsService) : INotificationHandler<PluginConfigurationChangedNotification>
{
    public Task Handle(PluginConfigurationChangedNotification notification, CancellationToken cancellationToken)
    {
        PluginConfigurationChangedStreamResponse message = new()
        {
            Name = notification.Instance.Plugin.Info.Name,
            Configuration = notification.Instance.Configuration.ToHellPluginConfigurationValues()
        };

        pluginsService.ConfigurationBroadcast.Broadcast(message);
        return Task.CompletedTask;
    }
}
