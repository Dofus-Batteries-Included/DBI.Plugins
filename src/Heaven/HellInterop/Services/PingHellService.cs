using DBI.Hell.HeavenInterop;
using Grpc.Core;

namespace DBI.Heaven.HellInterop.Services;

public class PingHellService : Ping.PingBase
{
    readonly ILogger<PingHellService> _logger;

    public PingHellService(ILogger<PingHellService> logger)
    {
        _logger = logger;
    }

    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        _logger.LogInformation("PING {Name}.", request.Name);
        return Task.FromResult(new PingResponse { Message = $"Ping {request.Name} successful." });
    }
}
