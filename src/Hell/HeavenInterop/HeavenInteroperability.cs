using System.Diagnostics;

namespace DBI.Hell.HeavenInterop;

static class HeavenInteroperability
{
    const string HeavenExecutableName = "../Heaven/Heaven.exe";

    public static HeavenHandle StartHeaven()
    {
        string thisAssemblyPath = typeof(CorePlugin).Assembly.Location;
        string? thisAssemblyDirectory = Path.GetDirectoryName(thisAssemblyPath);
        string heavenPath = Path.Join(thisAssemblyDirectory, HeavenExecutableName);

        if (!File.Exists(heavenPath))
        {
            throw new InvalidOperationException($"Could not find heaven at {heavenPath}.");
        }

        Process? heavenProcess = Process.Start(
            new ProcessStartInfo(heavenPath)
                { CreateNoWindow = true, ErrorDialog = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true }
        );
        if (heavenProcess == null)
        {
            throw new InvalidOperationException($"Could not start heaven executable at {heavenPath}.");
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            heavenProcess.Close();
            heavenProcess.WaitForExit();
        };

        return new HeavenHandle(heavenProcess);
    }
}
