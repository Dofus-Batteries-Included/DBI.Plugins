using System.Diagnostics;
using DBI.Hell.Helpers;
using Microsoft.Extensions.Logging;

namespace DBI.Hell;

// ReSharper disable once InconsistentNaming
public class GameClientInformation
{
    static readonly ILogger Log = CorePlugin.Logging.Create(typeof(ApplicationHelpers));

    GameClientInformation(Process process, string launcherPath)
    {
        ProcessId = process.Id;
        ExecutablePath = process.MainModule?.FileName;
        LauncherPath = launcherPath;
    }

    public int ProcessId { get; }
    public string ExecutablePath { get; }
    public string LauncherPath { get; }

    internal static GameClientInformation CreateFromOwnProcess()
    {
        Process ownProcess = Process.GetCurrentProcess();
        string findLauncherPath = FindLauncherPath();
        return new GameClientInformation(ownProcess, findLauncherPath);
    }

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
