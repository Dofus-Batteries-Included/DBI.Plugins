// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

const string heavenExecutableName = "Heaven.exe";
const int waitDelayInMilliseconds = 1000;
const string uniqueId = "Heaven_Launcher_e4917edf-0756-47c3-bd4c-2c25f0649979";

Thread.Sleep(Random.Shared.Next(100, 500));

Mutex mutex = new(false, uniqueId);
if (!mutex.WaitOne(waitDelayInMilliseconds))
{
    await Fail("Could not acquire lock.");
}

try
{
    Process[] heavenProcesses = Process.GetProcessesByName("Heaven");
    switch (heavenProcesses.Length)
    {
        case 0:
            Process heavenProcess = StartHeaven();
            await Console.Out.WriteLineAsync($"created:{heavenProcess.Id}");
            break;
        case 1:
            await Console.Out.WriteLineAsync($"existing:{heavenProcesses.Single().Id}");
            break;
        case > 1:
            await Fail("Multiple instances of Heaven are running.");
            break;
    }
}
finally
{
    mutex.ReleaseMutex();
}
return;

Process StartHeaven()
{
    string thisAssemblyPath = typeof(Program).Assembly.Location;
    string? thisAssemblyDirectory = Path.GetDirectoryName(thisAssemblyPath);
    string heavenPath = Path.Join(thisAssemblyDirectory, heavenExecutableName);

    if (!File.Exists(heavenPath))
    {
        throw new InvalidOperationException($"Could not find heaven at {heavenPath}.");
    }

    Process heavenProcess = new();
    heavenProcess.StartInfo.FileName = heavenPath;
    heavenProcess.StartInfo.CreateNoWindow = true;
    heavenProcess.StartInfo.ErrorDialog = true;
    heavenProcess.StartInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";

    if (!heavenProcess.Start())
    {
        throw new InvalidOperationException($"Could not start heaven executable at {heavenPath}.");
    }

    return heavenProcess;
}

async Task Fail(string errorMessage)
{
    await Console.Error.WriteLineAsync(errorMessage);
    Environment.Exit(1);
}
