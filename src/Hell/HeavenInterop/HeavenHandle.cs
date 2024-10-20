using System.Diagnostics;
using System.Net.Sockets;
using DBI.HellHeavenInterop;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace DBI.Hell.HeavenInterop;

public class HeavenHandle
{
    const string HeavenLauncherExecutableName = "Heaven Launcher.exe";

    readonly ILogger _logger = Hell.Logging.Create("HeavenHandle");
    public GrpcChannel Channel { get; private set; }

    internal HeavenHandle() { }

    public async Task<bool> ConnectToHeavenAsync()
    {
        _logger.LogInformation("Starting connection to Heaven...");

        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        string thisAssemblyPath = typeof(Hell).Assembly.Location;
        string thisAssemblyDirectory = Path.GetDirectoryName(thisAssemblyPath);
        string heavenLauncherPath = Path.Join(thisAssemblyDirectory, HeavenLauncherExecutableName);

        if (!File.Exists(heavenLauncherPath))
        {
            _logger.LogError("Could not find heaven launcher at {Path}.", heavenLauncherPath);
            return false;
        }

        string heavenSocketPath = await StartHeavenLauncherAndGetHeavenSocketPath(heavenLauncherPath);
        if (heavenSocketPath == null)
        {
            _logger.LogError("Could not find Heaven process.");
            return false;
        }

        Channel = CreateChannel(heavenSocketPath);
        _logger.LogInformation("Heaven interoperability socket configured: {Socket}.", heavenSocketPath);

        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await RegisterGameClientAsync();
                _logger.LogInformation("Successfully initialized connection to Heaven.");
                return true;
            }
            catch (Exception exn)
            {
                _logger.LogError(exn, "Error when trying to contact Heaven (attempt {Attempt}/{MaxAttempt}).", attempt, maxAttempts);
                await Task.Delay(1000);
            }
        }

        _logger.LogError("Could not connect to Heaven.");
        return false;
    }

    async Task<string> StartHeavenLauncherAndGetHeavenSocketPath(string heavenLauncherPath)
    {
        Process heavenLauncherProcess = new();
        heavenLauncherProcess.StartInfo.FileName = heavenLauncherPath;
        heavenLauncherProcess.StartInfo.CreateNoWindow = true;
        heavenLauncherProcess.StartInfo.ErrorDialog = true;
        heavenLauncherProcess.StartInfo.RedirectStandardOutput = true;
        heavenLauncherProcess.StartInfo.RedirectStandardError = true;
        heavenLauncherProcess.ErrorDataReceived += (_, args) => { _logger.LogError("Heaven Launcher: {Error}", args.Data); };

        bool started = heavenLauncherProcess.Start();

        if (!started)
        {
            return null;
        }

        heavenLauncherProcess.BeginErrorReadLine();

        string socketPath = null;
        do
        {
            string line = await heavenLauncherProcess.StandardOutput.ReadLineAsync();
            if (line == null)
            {
                break;
            }

            const string successSuffix = "success:";
            if (line.StartsWith(successSuffix))
            {
                socketPath = line[successSuffix.Length..];
                _logger.LogInformation("Found Heaven socket path: {Path}", socketPath);
            }

            _logger.LogInformation("Heaven Launcher: {Message}", line);
        } while (!heavenLauncherProcess.HasExited);

        _logger.LogInformation("Heaven Launcher has exited with code {ExitCode}.", heavenLauncherProcess.ExitCode);
        return socketPath;
    }

    GrpcChannel CreateChannel(string socketPath)
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

    async Task RegisterGameClientAsync()
    {
        if (Channel == null)
        {
            throw new InvalidOperationException("Heaven not started yet");
        }

        GameInstance.GameInstanceClient client = new(Channel);
        _logger.LogInformation("Registering game client {ProcessId}...", Hell.GameClientInformation.ProcessId);
        await client.RegisterGameClientAsync(new RegisterGameClientRequest { ProcessId = Hell.GameClientInformation.ProcessId });
    }
}
