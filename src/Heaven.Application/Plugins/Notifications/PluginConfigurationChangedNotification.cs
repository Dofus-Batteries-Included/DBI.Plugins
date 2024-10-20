using MediatR;

namespace DBI.Heaven.Application.Plugins.Notifications;

class PluginConfigurationChangedNotification : INotification
{
    public PluginConfigurationChangedNotification(PluginInstance instance)
    {
        Instance = instance;
    }

    public PluginInstance Instance { get; init; }
}
