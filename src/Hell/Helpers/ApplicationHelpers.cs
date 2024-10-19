using System.Diagnostics;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DBI.Hell.Helpers;

public static class ApplicationHelpers
{
    static readonly ILogger Log = CorePlugin.Logging.Create(typeof(ApplicationHelpers));

    public static void RestartAllClients()
    {
        Process[] clients = Process.GetProcessesByName("Dofus").Where(p => p.MainModule?.FileName == CorePlugin.GameClientInformation.ExecutablePath).ToArray();
        Log.LogInformation("All the game clients will be closed: {Clients}.", string.Join(", ", clients.Select(c => c.MainWindowTitle)));

        if (!string.IsNullOrWhiteSpace(CorePlugin.GameClientInformation.LauncherPath))
        {
            Process.Start(CorePlugin.GameClientInformation.LauncherPath);
        }
        else
        {
            Log.LogWarning("Could not find launcher path, the launcher will not be opened after closing the game clients.");
        }

        foreach (Process client in clients)
        {
            if (client.Id == CorePlugin.GameClientInformation.ProcessId)
            {
                continue;
            }

            Log.LogInformation("Closing {Name}...", client.MainWindowTitle);

            client.Kill();
            client.WaitForExit();
            client.Dispose();
        }

        Log.LogInformation("Closing self.");
        Application.Quit();
    }
}
