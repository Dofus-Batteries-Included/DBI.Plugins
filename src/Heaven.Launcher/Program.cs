// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

const string heavenExecutableName = "Heaven.exe";
const int waitDelayInMilliseconds = 1000;
const string uniqueId = "Heaven_Launcher_e4917edf-0756-47c3-bd4c-2c25f0649979";
const string heavenSocketNamePrefix = "dbi_heaven_";
string socketDirectory = Path.GetTempPath();

Mutex mutex = new(false, uniqueId);
if (!mutex.WaitOne(waitDelayInMilliseconds))
{
    throw new InvalidOperationException("Could not acquire lock.");
}

try
{
    Process[] heavenProcesses = Process.GetProcessesByName("Heaven");
    switch (heavenProcesses.Length)
    {
        case 0:
            Process newProcess = await StartHeaven();
            Success(newProcess);
            break;
        case 1:
            Process existingProcess = heavenProcesses.Single();
            Success(existingProcess);
            break;
        case > 1:
            throw new InvalidOperationException("Multiple instances of Heaven are running.");
    }
}
finally
{
    await Console.Out.FlushAsync();
}

return;

async Task<Process> StartHeaven()
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
    heavenProcess.StartInfo.RedirectStandardOutput = true;
    heavenProcess.StartInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
    heavenProcess.StartInfo.EnvironmentVariables["GRPC_SOCKET_DIR"] = socketDirectory;
    heavenProcess.StartInfo.EnvironmentVariables["GRPC_SOCKET_NAME_PREFIX"] = heavenSocketNamePrefix;

    if (!heavenProcess.Start())
    {
        throw new InvalidOperationException($"Could not start heaven executable at {heavenPath}.");
    }

    while (!heavenProcess.HasExited && await heavenProcess.StandardOutput.ReadLineAsync() is { } line)
    {
        if (line.Contains("Ready!"))
        {
            return heavenProcess;
        }
    }

    throw new InvalidOperationException("Could not start Heaven.");
}

void Success(Process process)
{
    string socketPath = Path.Combine(socketDirectory, $"{heavenSocketNamePrefix}{process.Id}");
    Console.WriteLine($"success:{socketPath}");
}
