﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using DofusBatteriesIncluded.Core.Metadata;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public abstract class DBIPlugin : BasePlugin
{
    protected new ILogger Log { get; }

    public DBIPlugin()
    {
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

    public virtual bool CanBeDisabled => true;

    public override sealed void Load()
    {
        Guid? expectedBuildId = GetExpectedBuildId();
        if (!expectedBuildId.HasValue)
        {
            Log.LogWarning("Could not determine expected build ID.");
        }
        else if (!DBI.DofusBuildId.HasValue)
        {
            Log.LogWarning("Could not determine actual build ID.");
        }
        else if (expectedBuildId.Value != DBI.DofusBuildId.Value)
        {
            Log.LogInformation(
                "Expected game build ID doesn't match actual build ID: {Expected} != {Actual}. "
                + "{Name} won't start, please download the version of the plugin that matches the version of the game.",
                expectedBuildId.Value,
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
            bool enabled = DBI.Configuration.Configure(Name, "Enabled", true).WithDescription($"Enable plugin {Name}").Hide().Bind();

            if (!enabled)
            {
                Log.LogInformation("{Name} is disabled.", Name);
                return;
            }
        }

        try
        {
            StartAsync().GetAwaiter().GetResult();
        }
        catch (Exception exn)
        {
            Log.LogError(exn, "Unexpected error while starting plugin {Name}: {Message}.\nYou should disable the plugin and restart the game.", Name, exn.Message);
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

    Guid? GetExpectedBuildId() => GetType().Assembly.GetCustomAttribute<ExpectedDofusBuildIdAttribute>()?.BuildId;
}
