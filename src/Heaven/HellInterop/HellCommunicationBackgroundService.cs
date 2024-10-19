using System.IO.Pipes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DBI.Heaven.HellInterop;

public class HellCommunicationBackgroundService : BackgroundService
{
    const string PipeName = "dbi_heaven_connect_pipe";
    readonly ILogger<HellCommunicationBackgroundService> _logger;

    public HellCommunicationBackgroundService(ILogger<HellCommunicationBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using NamedPipeServerStream serverStream = new(PipeName, PipeDirection.InOut);

        _logger.LogDebug("Named pipe {Name} ready.", PipeName);

        while (!stoppingToken.IsCancellationRequested)
        {
            await serverStream.WaitForConnectionAsync(stoppingToken);
        }
    }
}
