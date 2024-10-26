namespace DBI.Heaven.Application.Plugins;

class PluginInstancesService
{
    readonly Dictionary<string, PluginInstance> _instances = [];

    public IReadOnlyCollection<PluginInstance> GetInstances() => _instances.Values;

    public void RegisterInstance(PluginInstance instance) => _instances.Add(instance.Plugin.Info.Name, instance);

    public PluginInstance? GetInstance(string name) => _instances.GetValueOrDefault(name);
}
