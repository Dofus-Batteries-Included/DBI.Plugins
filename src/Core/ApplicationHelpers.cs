using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.Core;

public static class ApplicationHelpers
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(ApplicationHelpers));

    public static void RestartAllClients()
    {
        Process[] clients = Process.GetProcessesByName("Dofus").Where(p => p.MainModule?.FileName == DBI.GameClientInformation.ExecutablePath).ToArray();
        Log.LogInformation("All the game clients will be closed: {Clients}.", string.Join(", ", clients.Select(c => c.MainWindowTitle)));

        if (!string.IsNullOrWhiteSpace(DBI.GameClientInformation.LauncherPath))
        {
            Process.Start(DBI.GameClientInformation.LauncherPath);
        }
        else
        {
            Log.LogWarning("Could not find launcher path, the launcher will not be opened after closing the game clients.");
        }

        foreach (Process client in clients)
        {
            if (client.Id == DBI.GameClientInformation.ProcessId)
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
