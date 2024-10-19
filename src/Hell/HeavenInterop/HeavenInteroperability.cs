using System.Diagnostics;
using System.Net.Sockets;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace DBI.Hell.HeavenInterop;

static class HeavenInteroperability
{
    const string HeavenLauncherExecutableName = "Heaven Launcher.exe";

    static readonly ILogger Logger = CorePlugin.Logging.Create("HeavenInteroperability");
    static GrpcChannel? _channel;

    public static async Task<bool> StartHeaven()
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        string thisAssemblyPath = typeof(CorePlugin).Assembly.Location;
        string? thisAssemblyDirectory = Path.GetDirectoryName(thisAssemblyPath);
        string heavenLauncherPath = Path.Join(thisAssemblyDirectory, HeavenLauncherExecutableName);

        if (!File.Exists(heavenLauncherPath))
        {
            throw new InvalidOperationException($"Could not find heaven launcher at {heavenLauncherPath}.");
        }

        if (!StartHeavenLauncher(heavenLauncherPath, out Process heavenLauncherProcess))
        {
            throw new InvalidOperationException($"Could not start heaven launcher executable at {heavenLauncherPath}.");
        }

        int? heavenProcessId = await ReadHeavenProcessId(heavenLauncherProcess);
        if (!heavenProcessId.HasValue)
        {
            return false;
        }

        string socketPath = Path.Combine(Path.GetTempPath(), $"dbi_server_socket_{heavenProcessId}.tmp");
        _channel = CreateChannel(socketPath);
        Logger.LogInformation("Heaven interoperability socket configured: {Socket}.", socketPath);

        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await SendPingAsync();
                return true;
            }
            catch (Exception exn)
            {
                Logger.LogError(exn, "Error when trying to contact Heaven (attempt {Attempt}/{MaxAttempt}).", attempt, maxAttempts);
                await Task.Delay(1000);
            }
        }

        return false;
    }

    static bool StartHeavenLauncher(string path, out Process process)
    {
        process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.ErrorDialog = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += (_, args) => { Logger.LogError("Heaven Launcher: {Error}", args.Data); };

        bool started = process.Start();

        if (started)
        {
            process.BeginErrorReadLine();
        }

        return started;
    }

    static async Task<int?> ReadHeavenProcessId(Process heavenLauncherProcess)
    {
        int? heavenProcessId = null;
        do
        {
            string? line = await heavenLauncherProcess.StandardOutput.ReadLineAsync();
            if (line == null)
            {
                break;
            }

            const string createdSuffix = "created:";
            if (line.StartsWith(createdSuffix))
            {
                string idStr = line[createdSuffix.Length..];
                if (int.TryParse(idStr, out int id))
                {
                    Logger.LogInformation("Found Heaven process id: {Id}", id);
                    heavenProcessId = id;

                    await Task.Delay(1000);

                    continue;
                }
            }

            const string existingSuffix = "existing:";
            if (line.StartsWith(existingSuffix))
            {
                string idStr = line[existingSuffix.Length..];
                if (int.TryParse(idStr, out int id))
                {
                    Logger.LogInformation("Found Heaven process id: {Id}", id);
                    heavenProcessId = id;
                    continue;
                }
            }

            Logger.LogInformation("Heaven Launcher: {Message}", line);
        } while (!heavenLauncherProcess.HasExited);

        Logger.LogInformation("Heaven Launcher has exited with code {ExitCode}.", heavenLauncherProcess.ExitCode);
        return heavenProcessId;
    }

    static GrpcChannel CreateChannel(string socketPath)
    {
        UnixDomainSocketEndPoint udsEndPoint = new(socketPath);
        UnixDomainSocketsConnectionFactory connectionFactory = new(udsEndPoint);
        SocketsHttpHandler socketsHttpHandler = new()
        {
            ConnectCallback = connectionFactory.ConnectAsync
        };

        return GrpcChannel.ForAddress(
            "http://localhost:5001",
            new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler
            }
        );
    }

    static async Task SendPingAsync()
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Heaven not started yet");
        }

        Ping.PingClient client = new(_channel);
        Logger.LogInformation("Ping request...");
        PingResponse response = await client.PingAsync(new PingRequest { Name = "my name!!" });
        Logger.LogInformation("Ping response: {Message}.", response.Message);
    }
}
