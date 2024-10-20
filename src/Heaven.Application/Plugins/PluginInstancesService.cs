using MediatR;

namespace DBI.Heaven.Application.Plugins;

class PluginInstancesService(IMediator mediator)
{
    readonly Dictionary<string, PluginInstance> _instances = [];

    public IReadOnlyCollection<PluginInstance> GetInstances() => _instances.Values;

    public PluginInstance? RegisterInstance(PluginInstance instance)
    {
        _instances.Add(instance.Plugin.Info.Name, instance);
        return instance;
    }

    public PluginInstance? GetInstance(string name) => _instances.GetValueOrDefault(name);
}
