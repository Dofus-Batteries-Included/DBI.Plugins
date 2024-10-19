using System.IO.Pipes;

namespace DBI.Heaven.Application.HellInterop;

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
