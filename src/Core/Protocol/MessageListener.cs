using System;
using System.Threading.Tasks;

namespace DofusBatteriesIncluded.Core.Protocol;

public class MessageListener<T> : IMessageListener<T>
{
    public event EventHandler<T> MessageReceived;

    public Task HandleAsync(T message)
    {
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
