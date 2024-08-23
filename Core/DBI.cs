using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public static class DBI
{
    static DBI()
    {
        Enabled = Configuration.Configure("General", "Enabled", true).WithDescription("Enable or disable all Dofus Batteries Included plugins.").Hide().Bind();
        DofusBuildId = ReadDofusBuildId();
    }

    public static bool Enabled { get; internal set; }
    public static Guid? DofusBuildId { get; internal set; }
    public static DBIConfiguration Configuration { get; } = new();

    public static DBIPlugins Plugins { get; } = new();
    public static DBILogging Logging { get; } = new();

    public static DBICommands Commands { get; } = new();
    public static DBIMessaging Messaging { get; } = new();
    public static DBIPlayer Player { get; } = new();

    static readonly ILogger Log = Logging.Create(typeof(DBI));

    static Guid? ReadDofusBuildId()
    {
        const string bootConfigFilePathRelativeToDofusExe = "Dofus_Data/boot.config";
        string dofusExePath = FindDofusExePath();
        string bootConfigPath = Path.Join(dofusExePath, bootConfigFilePathRelativeToDofusExe);
        if (!File.Exists(bootConfigPath))
        {
            Log.LogWarning("Could not find boot.config, looked at: {Path}", bootConfigPath);
            return null;
        }

        Log.LogDebug("Found boot.config at {Path}", bootConfigPath);

        string[] lines = File.ReadAllLines(bootConfigPath);
        Dictionary<string, string> props = new();
        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            if (parts.Length != 2)
            {
                continue;
            }

            props[parts[0]] = parts[1];
        }

        string guidStr = props.GetValueOrDefault("build-guid");
        if (!Guid.TryParse(guidStr, out Guid guid))
        {
            Log.LogWarning("Could not find build-guid in boot.config file (path: {Path})", bootConfigPath);
            return null;
        }

        return guid;
    }

    static string FindDofusExePath()
    {
        string current = Path.GetDirectoryName(Path.GetFullPath(typeof(DBI).Assembly.Location));
        while (current != null && !File.Exists(Path.Join(current, "Dofus.exe")))
        {
            current = Path.GetDirectoryName(current);
        }

        return current;
    }
}
