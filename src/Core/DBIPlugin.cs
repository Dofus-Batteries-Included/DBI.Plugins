using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace DofusBatteriesIncluded.Core;

[BepInProcess("Dofus.exe")]
// ReSharper disable once InconsistentNaming
public abstract class DBIPlugin : BasePlugin
{
    protected new ILogger Log { get; }

    protected DBIPlugin(Guid? expectedBuildId = null)
    {
        ExpectedBuildId = expectedBuildId;
        BepInPlugin pluginAttribute = MetadataHelper.GetMetadata(GetType());
        if (pluginAttribute == null)
        {
            throw new InvalidOperationException("Expected plugin to have a [BepInPlugin] attrribute.");
        }

        Log = DBI.Logging.Create(GetType());
        Id = pluginAttribute.GUID;
        Name = pluginAttribute.Name;
        Version = pluginAttribute.Version?.ToString();
    }

    public string Id { get; }
    public string Name { get; }
    public string Version { get; }
    public bool Enabled { get; private set; }
    public PluginStatus Status { get; private set; }
    public Guid? ExpectedBuildId { get; }

    public virtual bool CanBeDisabled => true;

    public override sealed void Load()
    {
        if (!ExpectedBuildId.HasValue)
        {
            Log.LogWarning("Expected build ID was not provided, the plugin will run even if it has not been built against to correct game files.");
        }
        else
        {
            Log.LogDebug("Found expected build ID: {Expected}.", ExpectedBuildId.Value);
        }

        if (ExpectedBuildId.HasValue && DBI.DofusBuildId.HasValue && ExpectedBuildId.Value != DBI.DofusBuildId.Value)
        {
            Log.LogInformation(
                "Expected game build ID doesn't match actual build ID: {Expected} != {Actual}. "
                + "{Name} won't start, please download the version of the plugin that matches the version of the game.",
                ExpectedBuildId.Value,
                DBI.DofusBuildId.Value,
                Name
            );
            return;
        }

        if (!DBI.Enabled)
        {
            Log.LogInformation("Dofus Batteries Included is disabled.");
            return;
        }

        DBI.Plugins.Register(this);

        if (CanBeDisabled)
        {
            Enabled = DBI.Configuration.Configure(Name, "Enabled", true).WithDescription($"Enable plugin {Name}").Hide().RegisterChangeCallback(value => Enabled = value).Bind();

            if (!Enabled)
            {
                Log.LogInformation("{Name} is disabled.", Name);
                return;
            }
        }
        else
        {
            Enabled = true;
        }

        try
        {
            StartAsync().GetAwaiter().GetResult();
            Status = PluginStatus.Running;
        }
        catch (Exception exn)
        {
            Log.LogError(exn, "Unexpected error while starting plugin {Name}: {Message}.\nYou should disable the plugin and restart the game.", Name, exn.Message);
            Status = PluginStatus.FailedToStart;
        }
    }

    public override sealed bool Unload()
    {
        try
        {
            StopAsync().GetAwaiter().GetResult();
        }
        catch (Exception exn)
        {
            Log.LogError(exn, "Unexpected error while stopping plugin {Name}: {Message}.\nYou should disable the plugin and restart the game.", Name, exn.Message);
        }

        return base.Unload();
    }

    protected virtual Task StartAsync() => Task.CompletedTask;
    protected virtual Task StopAsync() => Task.CompletedTask;
}
