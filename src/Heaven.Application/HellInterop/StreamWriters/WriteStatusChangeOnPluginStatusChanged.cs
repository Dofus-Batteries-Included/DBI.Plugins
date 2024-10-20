using DBI.Heaven.Application.HellInterop.Services;
using DBI.Heaven.Application.Plugins.Notifications;
using DBI.HellHeavenInterop;
using MediatR;

namespace DBI.Heaven.Application.HellInterop.StreamWriters;

class WriteStatusChangeOnPluginStatusChanged(PluginsHellService pluginsService) : INotificationHandler<PluginStatusChangedNotification>
{
    public Task Handle(PluginStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        PluginStatusChangedStreamResponse message = new()
        {
            Name = notification.Instance.Plugin.Info.Name,
            Status = notification.Instance.ToHellStatus()
        };

        pluginsService.StatusBroadcast.Broadcast(message);
        return Task.CompletedTask;
    }
}
