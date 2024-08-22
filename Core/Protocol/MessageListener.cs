using System;

namespace DofusBatteriesIncluded.Core.Protocol;

public class MessageListener<T> : IDisposable
{
    public event EventHandler<T> MessageReceived;

    internal MessageListener()
    {
        Messaging.MessageReceived += SendMessage;
    }

    public void Dispose()
    {
        Messaging.MessageReceived -= SendMessage;
        GC.SuppressFinalize(this);
    }

    void SendMessage(object _, object message)
    {
        if (message is T value)
        {
            MessageReceived?.Invoke(this, value);
        }
    }
}
