using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ConnectionMessage = Com.Ankama.Dofus.Server.Connection.Protocol.Message;
using GameMessage = Com.Ankama.Dofus.Server.Game.Protocol.Message;

namespace DBI.Heaven.Application.HellInterop.Services;

public class MessagesHellService(ILogger<MessagesHellService> logger) : Messages.MessagesBase
{
    public override Task<Empty> MessageReceived(MessageRequest request, ServerCallContext context)
    {
        logger.LogInformation("Received message of length {Length}.", request.Content.Length);

        ConnectionMessage? connectionMessage = ConnectionMessage.Parser.ParseFrom(request.Content);
        logger.LogInformation("{Message}", connectionMessage);

        GameMessage? gameMessage = GameMessage.Parser.ParseFrom(request.Content);
        logger.LogInformation("{Message}", gameMessage);

        return Task.FromResult(new Empty());
    }
}
