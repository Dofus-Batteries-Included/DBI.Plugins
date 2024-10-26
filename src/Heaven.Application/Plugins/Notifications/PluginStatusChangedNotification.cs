using MediatR;

namespace DBI.Heaven.Application.Plugins.Notifications;

class PluginStatusChangedNotification : INotification
{
    public PluginStatusChangedNotification(PluginInstance instance, PluginInstance.PluginStatus oldStatus, PluginInstance.PluginStatus newStatus)
    {
        Instance = instance;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    public PluginInstance Instance { get; init; }
    public PluginInstance.PluginStatus OldStatus { get; init; }
    public PluginInstance.PluginStatus NewStatus { get; init; }
}
