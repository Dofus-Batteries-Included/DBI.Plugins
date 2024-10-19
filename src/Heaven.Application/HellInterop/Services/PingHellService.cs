using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DBI.Heaven.Application.HellInterop.Services;

public class PingHellService : GameInstance.GameInstanceBase
{
    readonly ILogger<PingHellService> _logger;

    public PingHellService(ILogger<PingHellService> logger)
    {
        _logger = logger;
    }

    public override Task<Empty> RegisterGameClient(RegisterGameClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("REGISTER CLIENT at process {ProcessId}.", request.ProcessId);
        return Task.FromResult(new Empty());
    }
}
