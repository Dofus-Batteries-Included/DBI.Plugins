using DBI.Hell.RedirectMessages;
using DBI.HellHeavenInterop;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace DBI.Hell.Messages;

public class MessagesManager
{
    static readonly ILogger Logger = Hell.Logging.Create(typeof(MessagesManager));

    public MessagesManager(MessageInterceptor interceptor)
    {
        interceptor.MessageReceived += (_, content) => OnMessageReceived(content);
    }

    static void OnMessageReceived(byte[] content)
    {
        HellHeavenInterop.Messages.MessagesClient client = new(Hell.Heaven.Channel);
        MessageRequest request = new() { Content = ByteString.CopyFrom(content) };

        try
        {
            client.MessageReceived(request);
        }
        catch (Exception exn)
        {
            Logger.LogError(exn, "Could not send message to Heaven: {Message}.", exn.Message);
        }
    }
}
