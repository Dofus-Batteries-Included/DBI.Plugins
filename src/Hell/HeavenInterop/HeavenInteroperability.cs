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

        Process heavenLauncherProcess = new();
        heavenLauncherProcess.StartInfo.FileName = heavenLauncherPath;
        heavenLauncherProcess.StartInfo.CreateNoWindow = true;
        heavenLauncherProcess.StartInfo.ErrorDialog = true;
        heavenLauncherProcess.StartInfo.RedirectStandardOutput = true;
        heavenLauncherProcess.StartInfo.RedirectStandardError = true;
        heavenLauncherProcess.ErrorDataReceived += (_, args) => { Logger.LogError("Heaven Launcher: {Error}", args.Data); };

        if (!heavenLauncherProcess.Start())
        {
            throw new InvalidOperationException($"Could not start heaven launcher executable at {heavenLauncherPath}.");
        }

        heavenLauncherProcess.BeginErrorReadLine();

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

        if (!heavenProcessId.HasValue)
        {
            return false;
        }

        string socketPath = Path.Combine(Path.GetTempPath(), $"dbi_server_socket_{heavenProcessId}.tmp");
        _channel = CreateChannel(socketPath);
        Logger.LogInformation("Heaven interoperability socket configured: {Socket}.", socketPath);

        try
        {
            SendPing().GetAwaiter().GetResult();
        }
        catch (Exception exn)
        {
            Logger.LogError(exn, "Error when trying to contact Heaven.");
            return false;
        }

        return true;
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

    static async Task SendPing()
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
