using System.Collections.Generic;

namespace DofusBatteriesIncluded.Plugins.Core;

// ReSharper disable once InconsistentNaming
public class DBIPlugins
{
    readonly HashSet<DBIPlugin> _plugins = [];

    internal DBIPlugins() { }

    public void Register(DBIPlugin plugin) => _plugins.Add(plugin);
    public IEnumerable<DBIPlugin> GetAll() => _plugins;
}
