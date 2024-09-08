using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Plugins.Core;

// ReSharper disable once InconsistentNaming
public class DBIGameClientInformation
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(ApplicationHelpers));

    internal DBIGameClientInformation()
    {
        Process ownProcess = Process.GetCurrentProcess();
        ProcessId = ownProcess.Id;
        ExecutablePath = ownProcess.MainModule?.FileName;
        LauncherPath = FindLauncherPath();
    }

    public int ProcessId { get; }
    public string ExecutablePath { get; }
    public string LauncherPath { get; }

    static string FindLauncherPath()
    {
        Process launcherProcess = Process.GetProcessesByName("Ankama Launcher").FirstOrDefault();
        string path = launcherProcess?.MainModule?.FileName;

        if (path == null)
        {
            Log.LogWarning("Could not find launcher path.");
            return null;
        }
        Log.LogInformation("Found launcher path: {Path}.", path);
        return path;
    }
}
