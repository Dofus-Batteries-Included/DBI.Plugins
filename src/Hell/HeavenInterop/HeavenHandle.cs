using System.Diagnostics;
using System.Net.Sockets;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace DBI.Hell.HeavenInterop;

class HeavenHandle
{
    const string HeavenLauncherExecutableName = "Heaven Launcher.exe";

    readonly ILogger _logger = CorePlugin.Logging.Create("HeavenHandle");
    public GrpcChannel Channel { get; private set; }

    public async Task<bool> ConnectToHeavenAsync()
    {
        _logger.LogInformation("Starting connection to Heaven...");

        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        string thisAssemblyPath = typeof(CorePlugin).Assembly.Location;
        string thisAssemblyDirectory = Path.GetDirectoryName(thisAssemblyPath);
        string heavenLauncherPath = Path.Join(thisAssemblyDirectory, HeavenLauncherExecutableName);

        if (!File.Exists(heavenLauncherPath))
        {
            _logger.LogError("Could not find heaven launcher at {Path}.", heavenLauncherPath);
            return false;
        }

        if (!StartHeavenLauncher(heavenLauncherPath, out Process heavenLauncherProcess))
        {
            _logger.LogError("Could not start heaven launcher executable at {Path}.", heavenLauncherPath);
            return false;
        }

        int? heavenProcessId = await ReadHeavenProcessId(heavenLauncherProcess);
        if (!heavenProcessId.HasValue)
        {
            _logger.LogError("Could not find Heaven process.");
            return false;
        }

        string socketPath = Path.Combine(Path.GetTempPath(), $"dbi_server_socket_{heavenProcessId}.tmp");
        Channel = CreateChannel(socketPath);
        _logger.LogInformation("Heaven interoperability socket configured: {Socket}.", socketPath);

        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await SendPingAsync();
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

    bool StartHeavenLauncher(string path, out Process process)
    {
        process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.ErrorDialog = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += (_, args) => { _logger.LogError("Heaven Launcher: {Error}", args.Data); };

        bool started = process.Start();

        if (started)
        {
            process.BeginErrorReadLine();
        }

        return started;
    }

    async Task<int?> ReadHeavenProcessId(Process heavenLauncherProcess)
    {
        int? heavenProcessId = null;
        do
        {
            string line = await heavenLauncherProcess.StandardOutput.ReadLineAsync();
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
                    _logger.LogInformation("Found Heaven process id: {Id}", id);
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
                    _logger.LogInformation("Found Heaven process id: {Id}", id);
                    heavenProcessId = id;
                    continue;
                }
            }

            _logger.LogInformation("Heaven Launcher: {Message}", line);
        } while (!heavenLauncherProcess.HasExited);

        _logger.LogInformation("Heaven Launcher has exited with code {ExitCode}.", heavenLauncherProcess.ExitCode);
        return heavenProcessId;
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

    async Task SendPingAsync()
    {
        if (Channel == null)
        {
            throw new InvalidOperationException("Heaven not started yet");
        }

        Ping.PingClient client = new(Channel);
        _logger.LogInformation("Ping request...");
        PingResponse response = await client.PingAsync(new PingRequest { Name = "my name!!" });
        _logger.LogInformation("Ping response: {Message}.", response.Message);
    }
}
