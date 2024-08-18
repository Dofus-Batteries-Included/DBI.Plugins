using System;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public abstract class DBIPlugin : BasePlugin
{
    public DBIPlugin()
    {
        BepInPlugin pluginAttribute = GetType().GetCustomAttribute<BepInPlugin>();
        if (pluginAttribute == null)
        {
            throw new InvalidOperationException("Expected plugin to have a [BepInPlugin] attrribute.");
        }

        Id = pluginAttribute.GUID;
        Name = pluginAttribute.Name;
        Version = pluginAttribute.Version?.ToString();
    }

    public string Id { get; }
    public string Name { get; }
    public string Version { get; }

    public override void Load()
    {
        DBI.Enabled = DBI.Configuration.Bind("General", "Master toggle", true, "Enable or disable all Dofus Batteries Included plugins.");

        if (!DBI.Enabled)
        {
            Log.LogInfo("Dofus Batteries Included is disabled.");
            return;
        }

        DBI.Plugins.Register(this);
    }
}
