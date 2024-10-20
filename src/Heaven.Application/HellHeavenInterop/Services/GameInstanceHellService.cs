using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DBI.Heaven.Application.HellHeavenInterop.Services;

public class GameInstanceHellService : GameInstance.GameInstanceBase
{
    readonly ILogger<GameInstanceHellService> _logger;

    public GameInstanceHellService(ILogger<GameInstanceHellService> logger)
    {
        _logger = logger;
    }

    public override Task<Empty> RegisterGameClient(RegisterGameClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("REGISTER CLIENT at process {ProcessId}.", request.ProcessId);
        return Task.FromResult(new Empty());
    }
}
