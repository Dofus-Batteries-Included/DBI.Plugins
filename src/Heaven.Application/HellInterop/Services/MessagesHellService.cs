using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DBI.Heaven.Application.HellInterop.Services;

public class MessagesHellService(ILogger<MessagesHellService> logger) : Messages.MessagesBase
{
    public override Task<Empty> MessageReceived(MessageRequest request, ServerCallContext context)
    {
        logger.LogInformation("Received message of length {Length}.", request.Content.Length);
        return Task.FromResult(new Empty());
    }
}
