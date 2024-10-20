using DBI.Heaven.Application.HellHeavenInterop.Services;
using DBI.Heaven.Application.Plugins.Notifications;
using MediatR;

namespace DBI.Heaven.Application.HellHeavenInterop.StreamWriters;

class WriteStatusChangeOnPluginStatusChanged(PluginsHellService pluginsService) : INotificationHandler<PluginStatusChangedNotification>
{
    public async Task Handle(PluginStatusChangedNotification notification, CancellationToken cancellationToken) =>
        await pluginsService.WriteStatusChange(notification, cancellationToken);
}
